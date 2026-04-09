using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations
{
    public class LocationCreateHandler : ICommandHandler<Guid, LocationCreateCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<LocationCreateCommand> _validator;
        private readonly ILogger<LocationCreateHandler> _logger;

        public LocationCreateHandler(
            ILocationsRepository locationsRepository,
            IValidator<LocationCreateCommand> validator,
            ILogger<LocationCreateHandler> logger)
        {
            _locationsRepository = locationsRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(LocationCreateCommand command, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<Guid>("validation error");

            var name = LocationName.Create(command.Dto.Name).Value;
            var locAddress = command.Dto.Address;
            var address = Address.Create(locAddress.HouseNumber, locAddress.Street, locAddress.City, locAddress.Country).Value;
            var timezone = Timezone.Create(command.Dto.Timezone).Value;

            var location = Location.Create(name, address, timezone).Value;

            var locRepo = await _locationsRepository.AddLocationAsync(location);

            if (!locRepo.IsSuccess)
            {
                _logger.LogInformation("failed to add location");
                return Result.Failure<Guid>("db saving problem");
            }

            _logger.LogInformation("location has been added");

            return location.Id.Value;
        }
    }
}
