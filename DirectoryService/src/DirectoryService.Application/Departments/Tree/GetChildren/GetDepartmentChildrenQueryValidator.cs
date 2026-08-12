using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetChildren
{
    public class GetDepartmentChildrenQueryValidator : AbstractValidator<GetDepartmentChildrenQuery>
    {
        public GetDepartmentChildrenQueryValidator()
        {
            RuleFor(x => x.ParentId)
                .NotEmpty()
                .WithError(Error.Validation(
                    code: "department.id.empty",
                    message: "Identifier cannot be empty.",
                    invalidField: "ParentId"));
        }
    }
}
