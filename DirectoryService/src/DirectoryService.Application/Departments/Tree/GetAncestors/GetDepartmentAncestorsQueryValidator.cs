using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetAncestors
{
    public class GetDepartmentAncestorsQueryValidator : AbstractValidator<GetDepartmentAncestorsQuery>
    {
        public GetDepartmentAncestorsQueryValidator()
        {
            RuleFor(x => x.ChildId)
                .NotEmpty()
                .WithError(Error.Validation(
                    code: "department.id.empty",
                    message: "Identifier cannot be empty..",
                    invalidField: "DepartmentId"));
        }
    }
}
