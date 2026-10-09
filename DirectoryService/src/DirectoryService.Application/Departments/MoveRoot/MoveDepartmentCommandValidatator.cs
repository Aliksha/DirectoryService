using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.MoveRoot
{
    public class MoveDepartmentCommandValidatator : AbstractValidator<MoveDepartmentCommand>
    {
        public MoveDepartmentCommandValidatator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("department.id.empty"));

            When(x => x.Dto.ParentId != null, () =>
            {
                RuleFor(x => x.Dto.ParentId)
                    .Must((command, parentId) => parentId != command.DepartmentId)
                    .WithError(Error.Validation(
                        code: "department.move.parent_is_self",
                        message: "Нельзя перенести подразделение в самого себя",
                        invalidField: "Dto.ParentId"));
            });
        }
    }
}
