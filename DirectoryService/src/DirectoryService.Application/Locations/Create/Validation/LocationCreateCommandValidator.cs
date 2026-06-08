using Core.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Create.Validation
{
    public class LocationCreateCommandValidator : AbstractValidator<LocationCreateDto>
    {
        public LocationCreateCommandValidator()
        {
            RuleFor(x => x.Name)
                .MustBeValueObject(LocationName.Create);

            RuleFor(x => x.Address)
                .MustBeValueObject<LocationCreateDto, AddressDto, Address>(x =>
                    Address.Create(x.HouseNumber, x.Street, x.City, x.Country));

            RuleFor(x => x.Timezone)
                .MustBeValueObject(Timezone.Create);

            //RuleFor(l => l.Dto.Name)
            //    .NotEmpty().WithMessage("Location name cannot be empty.")
            //    .MaximumLength(150).WithMessage("Location name cannot exceed 150 characters.")
            //    .MustAsync(async (command, name, cancellationToken) =>
            //    {
            //        bool isUnique = await locationsRepository.IsNameUniqueAsync(name, cancellationToken);
            //        return isUnique;
            //    }).WithMessage("A location with this name already exists.");

            //RuleFor(l => l.Dto.Address)
            //    .NotNull().WithMessage("Address details are required.");

            //RuleFor(l => l.Dto.Address.City)
            //    .NotEmpty().WithMessage("City is required.")
            //    .When(l => l.Dto.Address != null);

            //RuleFor(l => l.Dto.Address.Street)
            //    .NotEmpty().WithMessage("Street is required.")
            //    .When(l => l.Dto.Address != null);

            //RuleFor(l => l.Dto.Address.HouseNumber)
            //    .NotEmpty().WithMessage("House number is required.")
            //    .When(l => l.Dto.Address != null);

            //RuleFor(l => l.Dto.Address.Country)
            //    .NotEmpty().WithMessage("Country is required.")
            //    .When(l => l.Dto.Address != null);

            //RuleFor(l => l.Dto.Timezone)
            //    .NotEmpty().WithMessage("Timezone identifier is required.");
        }
    }
}
