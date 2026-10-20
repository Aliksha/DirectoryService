using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Departments.UpdateRoot;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Application.Departments.MoveRoot
{
    public class MoveDepartmentHandler : ICommandHandler<MovedDepartmentResponseDto, MoveDepartmentCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IValidator<MoveDepartmentCommand> _validator;
        private readonly ILogger<MoveDepartmentHandler> _logger;

        public MoveDepartmentHandler(
             IDepartmentsRepository departmentsRepository,
             ITransactionManager transactionManager,
             IDbConnectionFactory connectionFactory,
             IValidator<MoveDepartmentCommand> validator,
             ILogger<MoveDepartmentHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _connectionFactory = connectionFactory;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<MovedDepartmentResponseDto, Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorList();

            var departmentId = DepartmentId.Current(command.DepartmentId);
            var newParentId = command.Dto.ParentId;

            // 400 перенос в самого себя
            if (newParentId.HasValue && newParentId.Value == command.DepartmentId)
                return Error.Validation("department.move.parent_is_self", "Нельзя перенести подразделение в самого себя", "ParentId").ToErrors();

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
                return transactionScopeResult.Error.ToErrors();

            using var transactionScope = transactionScopeResult.Value;

            // живое соединение и транзакцию
            var currentTransaction = transactionScope.Transaction;
            var dbConnection = currentTransaction.Connection
                ?? throw new InvalidOperationException("Транзакция активна, но её соединение равно null.");
            try
            {
                // список id для блокировки, убираем null if parent — корень
                var idsToLock = new List<Guid> { command.DepartmentId };
                if (newParentId.HasValue)
                {
                    idsToLock.Add(newParentId.Value);
                }

                // ДЛЯ ИСКЛЮЧЕНИЯ DEADLOCKS, сортируем id по возрастанию
                var sortedIdsForLock = idsToLock.OrderBy(id => id).ToList();

                string lockSql = "SELECT id FROM public.departments WHERE id = ANY(@Ids) FOR UPDATE;";
                _ = await dbConnection.ExecuteAsync(lockSql, new { Ids = sortedIdsForLock }, transaction: currentTransaction);
            }
            // ловим исключение постгрес
            catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "40P01")
            {
                _logger.LogWarning(ex, "PostgreSQL deadlock detected (40P01) while locking departments {Ids}", newParentId.HasValue ? $"{command.DepartmentId} and {newParentId}" : $"{command.DepartmentId}");

                // Возвращаем 409 Conflict с требуемым кодом
                return Error.Conflict("department.move.conflict", "Конфликт транзакций или взаимная блокировка. Пожалуйста, обновите дерево или повторите операцию.").ToErrors();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire FOR UPDATE lock on departments");
                return Error.Failure("database.lock.failed", "Не удалось заблокировать структуру отделов для перемещения").ToErrors();
            }

            // двигаемый узел получаем
            var movingDepartment = await _departmentsRepository.GetBy(x => x.Id == departmentId, cancellationToken);
            if (movingDepartment == null)
                return GeneralErrors.NotFound(command.DepartmentId, "department.not_found").ToErrors();

            // если идемпотентность
            if (movingDepartment.ParentId?.Value == newParentId)
            {
                return new MovedDepartmentResponseDto(
                    movingDepartment.Id.Value,
                    // movingDepartment.ParentId?.Value ?? Guid.Empty,
                    movingDepartment.ParentId?.Value,
                    movingDepartment.Path.Value,
                    movingDepartment.Depth,
                    movingDepartment.UpdatedAt);
            }

            string newParentPathStr = "";
            int newParentDepth = 0;
            DepartmentId? newParentDomainId = null; // = DepartmentId.Current(newParentId.Value);
            Department? newParentDepartment = null;

            // если переносим внутрь другого узла
            if (newParentId.HasValue)
            {
                newParentDomainId = DepartmentId.Current(newParentId.Value);
                newParentDepartment = await _departmentsRepository.GetBy(x => x.Id == newParentDomainId, cancellationToken);

                if (newParentDepartment == null)
                {
                    return Error.Conflict("department.parent.not_found", "Новый родительский узел не найден").ToErrors();
                }

                if (!newParentDepartment.IsActive)
                {
                    return Error.Conflict("department.move.parent_deleted", "Нельзя перенести в удаленный родительский узел").ToErrors();
                }

                // валидация зацикливания
                bool isCycle = newParentDepartment.Path.Value == movingDepartment.Path.Value ||
                                newParentDepartment.Path.Value.StartsWith(movingDepartment.Path.Value + ".");
                if (isCycle)
                {
                    return Error.Conflict("department.move.cycle", "Зацикливание дерева.").ToErrors();
                }

                newParentPathStr = newParentDepartment.Path.Value;
                newParentDepth = newParentDepartment.Depth;
            }

            Path newDomainPath = newParentDepartment is not null
                ? Path.CreateChild(newParentDepartment.Path, movingDepartment.Identifier)
                : Path.CreateParent(movingDepartment.Identifier);

            var oldPathStr = movingDepartment.Path.Value;
            var newPathStr = newDomainPath.Value;

            var newDepth = newParentId.HasValue
                ? (short)(newParentDepth + 1)
                : (short)0;
            var updatedAt = DateTime.UtcNow;

            // if no parent -> передать null или пустой id домена
            var finalParentId = newParentDomainId;

            movingDepartment.Move(finalParentId, newDomainPath, newDepth); // т.к. Move is void

            var departmentUpdatedResult = await _departmentsRepository.UpdateAsync(movingDepartment, cancellationToken);
            if (departmentUpdatedResult.IsFailure)
            {
                _logger.LogInformation("failed to update department path structure");
                return Error.Failure(null, "db problem").ToErrors();
            }

            // bulk update with dapper (subpath + nlevel)
            // cинхронно сдвигаем всю ветку детей на стороне бд
            string bulkUpdateSql = """
                UPDATE public.departments
                SET 
                    path = (@NewParentPath::ltree || subpath(path, nlevel(@OldPath::ltree)))::ltree,
                    depth = nlevel((@NewParentPath::ltree || subpath(path, nlevel(@OldPath::ltree)))::ltree),
                    updated_at = @UpdatedAt
                WHERE path <@ @OldPath::ltree AND id <> @MovingId;
                """;

            var sqlParameters = new DynamicParameters();
            sqlParameters.Add("NewParentPath", newPathStr);
            sqlParameters.Add("OldPath", oldPathStr);
            sqlParameters.Add("MovingId", movingDepartment.Id.Value);
            sqlParameters.Add("UpdatedAt", DateTime.UtcNow);

            try
            {
                // достать транзакцию из scope наружу
                // var currentTransaction = transactionScope.Transaction;

                // достать то же самое живое соединение с постгрес на котором держится транзакция EF
                // var dbConnection = currentTransaction.Connection;

                await dbConnection!.ExecuteAsync(bulkUpdateSql, sqlParameters, transaction: currentTransaction);
            }
            // ловим дедлок при массовом обновлении детей
            catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "40P01")
            {
                _logger.LogWarning(ex, "PostgreSQL deadlock detected (40P01) during bulk subtree relocation for path {OldPath}", oldPathStr);
                return Error.Conflict("department.move.conflict", "Конфликт транзакций при обновлении поддерева. Пожалуйста, повторите операцию.").ToErrors();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dapper bulk update failed during subtree relocation");
                return Error.Failure("database.bulk_update.failed", "Ошибка при массовом обновлении путей поддерева").ToErrors();
            }

            var saveChangesAsync = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesAsync.IsFailure)
                return saveChangesAsync.Error.ToErrors();

            var commitedResult = transactionScope.Commit();
            if (commitedResult.IsFailure)
                return commitedResult.Error.ToErrors();

            _logger.LogInformation("department with id {departmentId} and its subtree has been moved", departmentId.Value);

            var response = new MovedDepartmentResponseDto(
                DepartmentId: movingDepartment.Id.Value,
               // ParentId: movingDepartment.ParentId is not null ? movingDepartment.ParentId.Value : Guid.Empty,
                ParentId: movingDepartment.ParentId?.Value,
                Path: newPathStr,
                Depth: newDepth,
                UpdatedAt: movingDepartment.UpdatedAt
            );

            return response;
        }
    }
}
