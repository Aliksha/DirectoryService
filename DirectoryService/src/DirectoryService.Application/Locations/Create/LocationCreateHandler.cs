using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Application.Locations.Create.Validation;
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
        private readonly IValidator<LocationCreateDto> _validator;
        private readonly ILogger<LocationCreateHandler> _logger;

        public LocationCreateHandler(
            ILocationsRepository locationsRepository,
            IValidator<LocationCreateDto> validator,
            ILogger<LocationCreateHandler> logger)
        {
            _locationsRepository = locationsRepository;
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

            _logger.LogInformation("location with id {location.Id} has been added", location.Value.Id.Value);

            return location.Value.Id.Value;
        }
    }
}
