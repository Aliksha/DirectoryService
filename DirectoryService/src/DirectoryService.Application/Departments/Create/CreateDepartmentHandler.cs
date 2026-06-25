using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DirectoryService.Application.Departments.Create
{
    public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IValidator<DepartmentCreateDto> _validator;
        private readonly ILogger<CreateDepartmentHandler> _logger;

        public CreateDepartmentHandler(
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            ITransactionManager transactionManager,
            IValidator<DepartmentCreateDto> validator,
            ILogger<CreateDepartmentHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorList();

            var departmentId = DepartmentId.Create();
            var departmentName = DepartmentName.Create(command.Dto.Name);
            var departmentIdentifier = Identifier.Create(command.Dto.Identifier);
            var parentId = command.Dto.ParentId;

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            // тут сделать  проверку на существование локации
            var chekExistingLocationResult = await _locationsRepository.CheckExisting(command.Dto.LocationsId, cancellationToken);
            if (chekExistingLocationResult.IsFailure)
                return chekExistingLocationResult.Error;

            // обязательная материализация коллекции .ToList
            var departmentLocations = command.Dto.LocationsId
                .Select(l => DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, LocationId.Current(l)))
                .ToList();

            Department department;

            // родительский / дочерий департамент
            if (command.Dto.ParentId == null)
            {
                var createParentResult = Department.CreateParent(departmentName.Value, departmentIdentifier.Value, departmentLocations);
                if (createParentResult.IsFailure)
                    return createParentResult.Error.ToErrors();

                department = createParentResult.Value;
            }
            else
            {
                var parentIdValue = command.Dto.ParentId.Value;
                var parentDepartmentId = DepartmentId.Current(parentIdValue);

                var parentDepartment = await _departmentsRepository
                    .GetBy(d => d.Id == parentDepartmentId, cancellationToken);

                if (parentDepartment == null)
                {
                    return GeneralErrors.NotFound(parentIdValue, "parent.department").ToErrors();
                }

                var childDepartmentResult = Department.CreateChild(departmentName.Value, departmentIdentifier.Value, parentDepartment, departmentLocations, departmentId);
                if (childDepartmentResult.IsFailure)
                    return childDepartmentResult.Error.ToErrors();

                department = childDepartmentResult.Value;
            }

            var departmentResult = await _departmentsRepository.AddAsync(department, cancellationToken);
            if (!departmentResult.IsSuccess)
            {
                _logger.LogInformation("failed to add department");
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
                commitedResult.Error.ToErrors();
            }

            _logger.LogInformation("Department with id {departmentId} has been added", departmentId.Value);

            return department.Id.Value;
        }
    }
}
