using DirectoryService.Application.Locations.Create.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Get
{
    public class GetLocationQueryValidator : AbstractValidator<LocationsGetQuery>
    {
        public GetLocationQueryValidator()
        {
            RuleFor(x => x.Dto.Page)
                .GreaterThan(0)
                .WithError(Error.Validation(
                    code: "pagination.page.invalid",
                    message: "page.number.too.short",
                    invalidField: "Dto.Page"))
                .When(x => x.Dto.Page.HasValue);

            RuleFor(x => x.Dto.PageSize)
                .GreaterThan(0)
                .WithError(Error.Validation(
                    code: "pagination.page_size.invalid",
                    message: "page.size.too.short",
                    invalidField: "Dto.PageSize"))
                .When(x => x.Dto.PageSize.HasValue);

            RuleFor(x => x.Dto.PageSize)
                .LessThanOrEqualTo(100)
                .WithError(Error.Validation(
                    code: "pagination.page_size.too_large",
                    message: "Page.size.too.big",
                    invalidField: "Dto.PageSize"))
                .When(x => x.Dto.PageSize.HasValue);
        }
    }

}
