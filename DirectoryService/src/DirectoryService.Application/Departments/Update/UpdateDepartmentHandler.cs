using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Update
{
    public class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IValidator<UpdateDepartmentCommand> _validator;
        private readonly ILogger<UpdateDepartmentHandler> _logger;

        public UpdateDepartmentHandler(
           IDepartmentsRepository departmentsRepository,
           ILocationsRepository locationsRepository,
           ITransactionManager transactionManager,
           IValidator<UpdateDepartmentCommand> validator,
           ILogger<UpdateDepartmentHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if(!validationResult.IsValid)
                return validationResult.ToErrorList();

            var departmentId = DepartmentId.Current(command.Dto.Id);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var departmentResult = await _departmentsRepository.GetBy(x => x.Id == departmentId, cancellationToken, x => x.Locations);
            if (departmentResult == null)
                return GeneralErrors.NotFound(departmentId.Value, "department").ToErrors();

            var department = departmentResult;

            if(!string.IsNullOrWhiteSpace(command.Dto.Name))
            {
                var newName = DepartmentName.Create(command.Dto.Name);
                if(newName.IsFailure)
                    return newName.Error.ToErrors();
                department.Rename(newName.Value);
            }

            if (!string.IsNullOrWhiteSpace(command.Dto.Identifier))
            {
                var newIdentifier = Identifier.Create(command.Dto.Identifier);
                if(newIdentifier.IsFailure)
                    return newIdentifier.Error.ToErrors();
                department.UpdateIdentifier(newIdentifier.Value);
            }

            if(command.Dto.LocationsId != null)
            {
                var newConnectionsWithLocations = new List<DepartmentLocation>();

                var checkExisting = await _locationsRepository.CheckExisting(command.Dto.LocationsId);
                if (checkExisting.IsFailure)
                    return checkExisting.Error;

                foreach(var locationGuid in command.Dto.LocationsId)
                {
                    var locationId = LocationId.Current(locationGuid);
                    var departmentLocationId = DepartmentLocationId.Create();

                    var departmentLocation = DepartmentLocation.Create(
                        departmentLocationId,
                        department.Id,
                        locationId);

                    newConnectionsWithLocations.Add(departmentLocation);
                }

                department.UpdateLocations(newConnectionsWithLocations);
            }

            var departmentUpdatedResult = await _departmentsRepository.UpdateAsync(department, cancellationToken);
            if (departmentUpdatedResult.IsFailure)
            {
                _logger.LogInformation("failed to update department");
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

            _logger.LogInformation("department with id {departmentId} has been updated", departmentId.Value);

            return department.Id.Value;
        }

    }
}
