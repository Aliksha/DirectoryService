using Core.Validation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Create
{
    public class CreatePositionCommandValidation : AbstractValidator<CreatePositionDto>
    {
        public CreatePositionCommandValidation()
        {
            RuleFor(x => x.Name)
                .MustBeValueObject(PositionName.Create);

            //RuleFor(p => p.Description) // vo ?
            //    .MustBeValueObject(Description.Create);

            _ = When(x => x.Departments != null, () =>
            {
                RuleFor(d => d.Departments)
                    .Must(departmentsIds => departmentsIds.Distinct().Count() == departmentsIds.Length)
                    .WithError(GeneralErrors.ValueIsInvalid("departments.ids.wrong.lenght"))
                    .NotEmpty()
                    .WithError(GeneralErrors.ValueIsRequired("departments.ids.empty"));
            });
        }
    }
}
