using DirectoryService.Domain.Locations;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations
{
    public class LocationCreateCommandValidator : AbstractValidator<LocationCreateCommand>
    {
        public LocationCreateCommandValidator()
        {
            RuleFor(l => l.Dto.Name).NotEmpty();

            RuleFor(l => l.Dto.Address).NotEmpty();

            RuleFor(l => l.Dto.Timezone).NotEmpty();
        }
    }
}
