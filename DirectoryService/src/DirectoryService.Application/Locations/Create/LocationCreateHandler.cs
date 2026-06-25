using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DirectoryService.Application.Locations.Create
{
    public class LocationCreateHandler : ICommandHandler<Guid, LocationCreateCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IValidator<LocationCreateDto> _validator;
        private readonly ILogger<LocationCreateHandler> _logger;

        public LocationCreateHandler(
            ILocationsRepository locationsRepository,
            ITransactionManager transactionManager,
            IValidator<LocationCreateDto> validator,
            ILogger<LocationCreateHandler> logger)
        {
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(LocationCreateCommand command, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                // return GeneralErrors.ValueIsInvalid("location").ToErrors();
                return validationResult.ToErrorList();
            }

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var chekingNameUnique = await _locationsRepository.IsNameUniqueAsync(command.Dto.Name, cancellationToken);
            if(chekingNameUnique == false)
            {
                 _logger.LogInformation("name already exists");
                 return Error.Conflict(null, "name.conflict").ToErrors();
            }

            var name = LocationName.Create(command.Dto.Name);
            var locationAddress = command.Dto.Address;
            var address = Address.Create(locationAddress.HouseNumber, locationAddress.Street, locationAddress.City, locationAddress.Country);
            var timezone = Timezone.Create(command.Dto.Timezone);

            var location = Location.Create(name.Value, address.Value, timezone.Value);

            var repositoryResult = await _locationsRepository.AddLocationAsync(location.Value);

            if (!repositoryResult.IsSuccess)
            {
                _logger.LogInformation("failed to add location");
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

            _logger.LogInformation("location with id {location.Id} has been added", location.Value.Id.Value);

            return location.Value.Id.Value;
        }
    }
}
