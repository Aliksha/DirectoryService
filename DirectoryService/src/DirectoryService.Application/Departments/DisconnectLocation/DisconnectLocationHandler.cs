using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.Departments.ConnectToLocation;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.DisconnectLocation
{
    public class DisconnectLocationHandler : ICommandHandler<Guid, DisconnectLocationCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentLocationsRepository _departmentLocationsRepository;
        private readonly ILogger<DisconnectLocationHandler> _logger;

        public DisconnectLocationHandler(
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            ITransactionManager transactionManager,
            IDepartmentLocationsRepository departmentLocationsRepository,
            ILogger<DisconnectLocationHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _departmentLocationsRepository = departmentLocationsRepository;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(DisconnectLocationCommand command, CancellationToken cancellationToken = default)
        {
            var departmentId = DepartmentId.Current(command.Dto.DepartmentId);
            var locationId = LocationId.Current(command.Dto.LocationId);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var checkDepartmentExists = await _departmentsRepository.CheckExisting(new[] { departmentId.Value }, cancellationToken);
            if (checkDepartmentExists.IsFailure)
                return checkDepartmentExists.Error;

            var checkLocationExists = await _locationsRepository.CheckExisting(new[] { locationId.Value }, cancellationToken);
            if(checkLocationExists.IsFailure)
                return checkLocationExists.Error;

            bool checkConnectionExists = await _departmentLocationsRepository.IsConnectedAlready(departmentId, locationId, cancellationToken);
            if(!checkConnectionExists)
                return GeneralErrors.ValueIsInvalid("department.location.not.connected").ToErrors();

            var connectionResult = await _departmentLocationsRepository.DeleteConnectionAsync(departmentId, locationId, cancellationToken);
            if (connectionResult.IsFailure)
            {
                _logger.LogInformation("failed to delete department location connection");
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

            return departmentId.Value;
        }
    }
}
