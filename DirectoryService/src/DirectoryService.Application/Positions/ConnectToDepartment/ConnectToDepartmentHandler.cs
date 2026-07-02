using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.ConnectToDepartment
{
    public class ConnectToDepartmentHandler : ICommandHandler<Guid, ConnectToDepartmentCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IDepartmentPositionsRepository _departmentPositionsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<ConnectToDepartmentHandler> _logger;

        public ConnectToDepartmentHandler(
            IPositionsRepository positionsRepository,
            IDepartmentsRepository departmentsRepository,
            IDepartmentPositionsRepository departmentPositionsRepository,
            ITransactionManager transactionManager,
            ILogger<ConnectToDepartmentHandler> logger)
        {
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _departmentPositionsRepository = departmentPositionsRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(ConnectToDepartmentCommand command, CancellationToken cancellationToken = default)
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
            {
                return checkPositionExists.Error;
            }

            var checkDepartmentExists = await _departmentsRepository.CheckExisting(new[] {departmentId.Value}, cancellationToken);
            if (checkDepartmentExists.IsFailure)
            {
                return checkDepartmentExists.Error;
            }

            bool isConnectedAlready = await _departmentPositionsRepository.IsConnectedAlready(departmentId, positionId, cancellationToken);
            if (isConnectedAlready)
            {
                return GeneralErrors.ValueIsInvalid("department.position.already.connected").ToErrors();
            }

            var departmentPositionId = DepartmentPositionId.Create();

            var newConnection = DepartmentPosition.Create(departmentPositionId, departmentId, positionId);

            var addConnection = await _departmentPositionsRepository.AddConnectionAsync(newConnection, cancellationToken);
            if (addConnection.IsFailure)
            {
                _logger.LogInformation("failed to add department position connection");
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

            return departmentPositionId.Value;
        }
    }
}
