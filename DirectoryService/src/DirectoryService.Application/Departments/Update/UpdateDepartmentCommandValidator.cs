using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Update
{
    public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {

            _ = When(x => !string.IsNullOrWhiteSpace(x.Dto.Name), () =>
            {

                RuleFor(x => x.Dto.Name)
                    .MustBeValueObject(DepartmentName.Create);
            });

            _ = When(x => !string.IsNullOrWhiteSpace(x.Dto.Identifier), () =>
            {

                RuleFor(x => x.Dto.Identifier)
                    .MustBeValueObject(Identifier.Create);
            });

            _ = When(x => x.Dto.LocationsId != null, () =>
            {
                RuleFor(d => d.Dto.LocationsId)
                    .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
                    .WithError(GeneralErrors.ValueIsInvalid("location.ids.wrong.lenght"))
                    .NotEmpty()
                    .WithError(GeneralErrors.ValueIsRequired("location.ids.empty"));
            });
        }
    }
}
