using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.GetByDapper
{
    public class GetLocationsQueryValidator : AbstractValidator<GetLocationsQuery>
    {
        private static readonly string[] AllowedSortFields = { "name", "createdat", "created", "departmentcount" };
        private static readonly string[] AllowedSortDirs = { "asc", "desc" };

        public GetLocationsQueryValidator()
        {
            RuleFor(x => x.Dto.Page)
                .GreaterThan(0)
                .WithError(Error.Validation(
                    code: "pagination.page.invalid",
                    message: "Номер страницы должен быть больше или равен 1.",
                    invalidField: "Dto.Page"))
                .When(x => x.Dto.Page.HasValue);

            RuleFor(x => x.Dto.PageSize)
                .InclusiveBetween(1, 100)
                .WithError(Error.Validation(
                    code: "pagination.page_size.invalid",
                    message: "Размер страницы должен быть от 1 до 100 элементов.",
                    invalidField: "Dto.PageSize"))
                .When(x => x.Dto.PageSize.HasValue);

            RuleFor(x => x.Dto.MinDepartmentCount)
                .GreaterThanOrEqualTo(0)
                .WithError(Error.Validation(
                    code: "filters.min_department_count.invalid",
                    message: "Минимальное количество департаментов не может быть отрицательным.",
                    invalidField: "Dto.MinDepartmentCount"))
                .When(x => x.Dto.MinDepartmentCount.HasValue);

            // неизвестное поле сортировки падает на этапе валидации 400
            RuleFor(x => x.Dto.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || AllowedSortFields.Contains(sortBy.ToLower().Trim()))
                .WithError(Error.Validation(
                    code: "sorting.field.invalid",
                    message: "Недопустимое поле сортировки. Разрешены только name, createdAt, departmentCount.",
                    invalidField: "Dto.SortBy"));

            RuleFor(x => x.Dto.SortDir)
                .Must(dir => string.IsNullOrEmpty(dir) || AllowedSortDirs.Contains(dir.ToLower().Trim()))
                .WithError(Error.Validation(
                    code: "sorting.direction.invalid",
                    message: "Направление сортировки может быть только asc или desc.",
                    invalidField: "Dto.SortDir"));
        }
    }
}
