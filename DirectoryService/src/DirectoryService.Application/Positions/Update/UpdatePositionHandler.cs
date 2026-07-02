using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Update
{
    public class UpdatePositionHandler : ICommandHandler<Guid, UpdatePositionCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IValidator<UpdatePositionCommand> _validator;
        private readonly ILogger<UpdatePositionHandler> _logger;

        public UpdatePositionHandler(
            IPositionsRepository positionsRepository,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            IValidator<UpdatePositionCommand> validator,
            ILogger<UpdatePositionHandler> logger)
        {
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorList();

            var positionId = PositionId.Current(command.Dto.Id);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var positionResult = await _positionsRepository.GetById(positionId, cancellationToken);
            if (positionResult.IsFailure)
            {
                _logger.LogInformation("failed to get position");
                return Error.NotFound(null, "not found").ToErrors();
            }

            var position = positionResult.Value;

            if (!string.IsNullOrWhiteSpace(command.Dto.Name))
            {
                var nameResult = PositionName.Create(command.Dto.Name);
                if (nameResult.IsFailure)
                    return nameResult.Error.ToErrors();

                // domain method
                position.Rename(nameResult.Value);
            }

            if (command.Dto.Description != null)
            {
                position.UpdateDescription(command.Dto.Description);
            }

            if (command.Dto.DepartmentsId != null)
            {
                var newConnectionsWithDepartments = new List<DepartmentPosition>();

                var checkExisting = await _departmentsRepository.CheckExisting(command.Dto.DepartmentsId);
                if (checkExisting.IsFailure)
                    return checkExisting.Error;

                foreach(var departmentGuid in command.Dto.DepartmentsId)
                {
                    var departmentId = DepartmentId.Current(departmentGuid);
                    var positionDepartmentId = DepartmentPositionId.Create();

                    var positionDepartment = DepartmentPosition.Create(
                        positionDepartmentId,
                        departmentId,
                        positionId);

                    newConnectionsWithDepartments.Add(positionDepartment);
                }

                position.UpdateDepartments(newConnectionsWithDepartments);
            }

            var positionUpdatedResult = await _positionsRepository.UpdateAsync(position, cancellationToken);
            if (!positionUpdatedResult.IsSuccess)
            {
                _logger.LogInformation("failed to update position");
                return Error.Failure(null, "db problem").ToErrors();
            }

            var saveChangesAsync = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesAsync.IsFailure)
            {
                return saveChangesAsync.Error.ToErrors();
            }

            var commitedResult = transactionScope.Commit();
            if (commitedResult.IsFailure)
            {
                return commitedResult.Error.ToErrors();
            }

            _logger.LogInformation("Position with id {positionId} has been updated", positionId.Value);

            return position.Id.Value;
        }
    }

}
