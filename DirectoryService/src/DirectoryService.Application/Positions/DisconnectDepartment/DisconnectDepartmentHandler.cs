using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.DisconnectDepartment
{
    public class DisconnectPositionHandler : ICommandHandler<Guid, DisconnectDepartmentCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentPositionsRepository _departmentPositionsRepository;
        private readonly ILogger<DisconnectPositionHandler> _logger;

        public DisconnectPositionHandler(
            IPositionsRepository positionsRepository,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            IDepartmentPositionsRepository departmentPositionsRepository,
            ILogger<DisconnectPositionHandler> logger)
        {
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _departmentPositionsRepository = departmentPositionsRepository;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(DisconnectDepartmentCommand command, CancellationToken cancellationToken = default)
        {
            var positionId = PositionId.Current(command.Dto.PositionId);
            var departmentId = DepartmentId.Current(command.Dto.DepartmentId);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var checkPositionExists = await _positionsRepository.CheckExisting(new[] { positionId.Value }, cancellationToken);
            if (checkPositionExists.IsFailure)
                return checkPositionExists.Error;

            var checkDepartmentExists = await _departmentsRepository.CheckExisting(new[] { departmentId.Value }, cancellationToken);
            if (checkDepartmentExists.IsFailure)
                return checkDepartmentExists.Error;

            bool checkConnectionExists = await _departmentPositionsRepository.IsConnectedAlready(departmentId, positionId, cancellationToken);
            if (!checkConnectionExists)
                return GeneralErrors.ValueIsInvalid("department.position.not.connected").ToErrors();

            var connectionResult = await _departmentPositionsRepository.DeleteConnectionAsync(departmentId, positionId, cancellationToken);
            if (connectionResult.IsFailure)
            {
                _logger.LogInformation("failed to delete department position connection");
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

            _logger.LogInformation("Position {positionId} has been disconnected from department {departmentId}", positionId.Value, departmentId.Value);

            return departmentId.Value;
        }
    }

}
