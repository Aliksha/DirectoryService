using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Update
{
    public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
    {
        public UpdatePositionCommandValidator()
        {

            _ = When(x => !string.IsNullOrWhiteSpace(x.Dto.Name), () =>
            {

                RuleFor(x => x.Dto.Name)
                    .MustBeValueObject(PositionName.Create);
            });

            _ = When(x => x.Dto.DepartmentsId != null, () =>
            {
                RuleFor(d => d.Dto.DepartmentsId)
                    .Must(ids => ids.Distinct().Count() == ids.Length)
                    .WithError(GeneralErrors.ValueIsInvalid("departments.ids.wrong.length"));
            });
        }
    }
}
