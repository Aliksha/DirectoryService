using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.ConnectToLocation
{
    public class ConnectToLocationHandler : ICommandHandler<Guid, ConnectToLocationCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IDepartmentLocationsRepository _departmentLocationsRepository;
        private readonly ILogger<ConnectToLocationHandler> _logger;

        public ConnectToLocationHandler(
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IDepartmentLocationsRepository departmentLocationsRepository,
            ILogger<ConnectToLocationHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _departmentLocationsRepository = departmentLocationsRepository;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(ConnectToLocationCommand command, CancellationToken cancellationToken = default)
        {
            var departmentId = DepartmentId.Current(command.Dto.DepartmentId);
            var locationId = LocationId.Current(command.Dto.LocationId);

            var checkDepartment = await _departmentsRepository.CheckExisting(new[] { departmentId.Value }, cancellationToken);
            if(checkDepartment.IsFailure)
            {
                return checkDepartment.Error;
            }

            var checkLocation = await _locationsRepository.CheckExisting(new[] { locationId.Value }, cancellationToken);
            if(checkLocation.IsFailure)
            {
                return checkLocation.Error;
            }

            bool connectionAlreadyExists = await _departmentLocationsRepository.IsConnectedAlready(departmentId, locationId, cancellationToken);
            if (connectionAlreadyExists)
                return GeneralErrors.ValueIsInvalid("department.location.already.connected").ToErrors();

            var departmentLocationId = DepartmentLocationId.Create();

            var newConnection = DepartmentLocation.Create(departmentLocationId, departmentId, locationId);

            var addConnection = await _departmentLocationsRepository.AddConnectionAsync(newConnection, cancellationToken);
            if (addConnection.IsFailure)
            {
                _logger.LogInformation("failed to add department location connection");
                return Error.Failure(null, "db problem").ToErrors();
            }

            // можно выгрузить department с бд и вызвать доменный Touch - поменять UpdatedAt

            return departmentLocationId.Value;
        }
    }
}
