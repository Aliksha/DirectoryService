using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetSearch
{
    public class SearchDepartmentsTreeQueryValidator : AbstractValidator<SearchDepartmentsTreeQuery>
    {
        public SearchDepartmentsTreeQueryValidator()
        {
            RuleFor(x => x.Q)
                .NotEmpty()
                .WithError(Error.Validation(
                    code: "tree.search.empty",
                    message: "search cannot be empty.",
                    invalidField: "q"));

            RuleFor(x => x.Q)
                .MinimumLength(2)
                .WithError(Error.Validation(
                    code: "tree.search.too_short",
                    message: "min 2 symbols.",
                    invalidField: "q"));
        }
    }
}
