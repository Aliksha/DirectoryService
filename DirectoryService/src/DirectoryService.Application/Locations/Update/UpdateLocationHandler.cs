using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Update
{
    public class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<UpdateLocationCommand> _validator;
        private readonly ILogger<UpdateLocationHandler> _logger;

        public UpdateLocationHandler(ILocationsRepository locationsRepository, IValidator<UpdateLocationCommand> validator, ILogger<UpdateLocationHandler> logger)
        {
            _locationsRepository = locationsRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if(!validationResult.IsValid)
                return validationResult.ToErrorList();

            var locationId = LocationId.Current(command.Dto.Id);
            var locationResult = await _locationsRepository.GetById(locationId, cancellationToken);
            if(locationResult.IsFailure)
            {
                _logger.LogInformation("failed to get location");
                return Error.NotFound(null, "not found").ToErrors();
            }

            var location = locationResult.Value;

            if (!string.IsNullOrWhiteSpace(command.Dto.Name))
            {
                var nameResult = LocationName.Create(command.Dto.Name);
                if (nameResult.IsFailure)
                    return nameResult.Error.ToErrors();

                location.Rename(nameResult.Value);
            }

            if (command.Dto.Address != null) // проверка объекта, а не строки!
            {
                var addressResult = Address.Create(
                    command.Dto.Address.HouseNumber,
                    command.Dto.Address.Street,
                    command.Dto.Address.City,
                    command.Dto.Address.Country);

                if (addressResult.IsFailure)
                    return addressResult.Error; // здесь возвращается Errors

                location.ChangeAddress(addressResult.Value);
            }

            if (!string.IsNullOrWhiteSpace(command.Dto.Timezone))
            {
                var timezoneResult = Timezone.Create(command.Dto.Timezone);
                if (timezoneResult.IsFailure)
                    return timezoneResult.Error.ToErrors();

                location.ChangeTimezone(timezoneResult.Value);
            }

            // eсли добавите флаг IsActive в dto :
            // if (command.Dto.IsActive.HasValue)
            // {
            //     if (command.Dto.IsActive.Value) location.Activate();
            //     else location.Deactivate();
            // }

            var locationUpdatedResult = await _locationsRepository.UpdateAsync(location, cancellationToken);
            if (!locationUpdatedResult.IsSuccess)
            {
                _logger.LogInformation("failed to update location");
                return Error.Failure(null, "db problem").ToErrors();
            }

            _logger.LogInformation("Location with id {locationId} has been updated", locationId.Value);

            return location.Id.Value;
        }
    }
}
