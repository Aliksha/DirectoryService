using Core.Validation;
using FluentValidation;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Get
{
    public class GetDepartmentsQueryValidator : AbstractValidator<GetDepartmentsQuery>
    {
        private static readonly string[] AllowedSortFields = ["name", "created", "createdat"];
        private static readonly string[] AllowedSortDirs = ["asc", "desc", null, ""];

        public GetDepartmentsQueryValidator()
        {
            // валидация номера страницы page < 1
            RuleFor(x => x.Dto.Page)
                .GreaterThan(0)
                .WithError(Error.Validation(
                    code: "pagination.page.invalid",
                    message: "Номер страницы должен быть больше или равен 1.",
                    invalidField: "Dto.Page"))
                .When(x => x.Dto.Page.HasValue);

            // валидация размера страницы, мин 1
            RuleFor(x => x.Dto.PageSize)
                .GreaterThan(0)
                .WithError(Error.Validation(
                    code: "pagination.page_size.too_short",
                    message: "Размер страницы должен быть больше 0.",
                    invalidField: "Dto.PageSize"))
                .When(x => x.Dto.PageSize.HasValue);

            // валидация максимального размера страницы, мах 100
            RuleFor(x => x.Dto.PageSize)
                .LessThanOrEqualTo(100)
                .WithError(Error.Validation(
                    code: "pagination.page_size.too_large",
                    message: "Размер страницы не может превышать 100 элементов.",
                    invalidField: "Dto.PageSize"))
                .When(x => x.Dto.PageSize.HasValue);

            // слишком длинный search
            RuleFor(x => x.Dto.Search)
                .MaximumLength(100)
                .WithError(Error.Validation(
                    code: "search.query.too_long",
                    message: "Поисковый запрос слишком длинный.",
                    invalidField: "Dto.Search"))
                .When(x => !string.IsNullOrEmpty(x.Dto.Search));

            // неизвестное поле сортировки возвращает 400
            RuleFor(x => x.Dto.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || AllowedSortFields.Contains(sortBy.ToLower().Trim()))
                .WithError(Error.Validation(
                    code: "sorting.field.invalid",
                    message: "Недопустимое поле сортировки. Разрешены только: name, createdAt.",
                    invalidField: "Dto.SortBy"));

            // для направления сортировки (sortDir = asc|desc)
            RuleFor(x => x.Dto.SortDir)
                .Must(dir => string.IsNullOrEmpty(dir) || AllowedSortDirs.Contains(dir.ToLower().Trim()))
                .WithError(Error.Validation(
                    code: "sorting.direction.invalid",
                    message: "Направление сортировки может быть только 'asc' или 'desc'.",
                    invalidField: "Dto.SortDir"));
        }
    }

}
