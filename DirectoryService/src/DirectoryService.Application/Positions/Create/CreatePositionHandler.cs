using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Create
{
    public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IValidator<CreatePositionDto> _validator;
        private readonly ILogger<CreatePositionHandler> _logger;

        public CreatePositionHandler(
            IPositionsRepository positionsRepository,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            IValidator<CreatePositionDto> validator,
            ILogger<CreatePositionHandler> logger)
        {
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken = default)
        {
            var validarionResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
            if (!validarionResult.IsValid)
            {
                return validarionResult.ToErrorList();
            }

            var positionId = PositionId.Create();
            var positionName = PositionName.Create(command.Dto.Name);
            var positionDescription = command.Dto.Description;

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var connectionsPositionDepartments = new List<DepartmentPosition>();

            if(command.Dto.Departments != null && command.Dto.Departments.Any())
            {
                // тут сделать проверку на существование
                var existingDepartmentsCheck = await _departmentsRepository.CheckExisting(command.Dto.Departments, cancellationToken);
                if (existingDepartmentsCheck.IsFailure)
                    return existingDepartmentsCheck.Error;

                connectionsPositionDepartments = command.Dto.Departments
                    .Select(x => DepartmentPosition.Create(DepartmentPositionId.Create(), DepartmentId.Current(x), positionId))
                    .ToList();
            }

            var position = Position.Create(positionName.Value, positionDescription, connectionsPositionDepartments, positionId);

            var repositoryResult = await _positionsRepository.Add(position.Value, cancellationToken);
            if (!repositoryResult.IsSuccess)
            {
                _logger.LogInformation("failed to add position");
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

            _logger.LogInformation("Position whith id {positionId} has been created", positionId.Value);

            return positionId.Value;
        }
    }
}
