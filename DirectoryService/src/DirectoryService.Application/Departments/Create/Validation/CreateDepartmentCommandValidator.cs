using Core.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Create.Validation
{
    public class CreateDepartmentCommandValidator : AbstractValidator<DepartmentCreateDto>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.Name)
                .MustBeValueObject(DepartmentName.Create);

            RuleFor(x => x.Identifier)
                .MustBeValueObject(Identifier.Create);

            //RuleFor(d => d.ParentId)
            //    .Must(parentId => parentId == null || parentId != Guid.Empty)
            //    .WithError(GeneralErrors.ValueIsRequired("parent.id.empty"));

            RuleFor(d => d.LocationsId)
                .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
                .WithError(GeneralErrors.ValueIsInvalid("location.ids.wrong.lenght"));
        }
    }
}
