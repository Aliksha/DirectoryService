using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Get
{
    public class GetDepartmentsHandler : IQueryHandler<DepartmentResponseDto, GetDepartmentsQuery>
    {
        private readonly IReadDbContext _context;
        private readonly IValidator<GetDepartmentsQuery> _validator;

        public GetDepartmentsHandler(IReadDbContext context, IValidator<GetDepartmentsQuery> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<DepartmentResponseDto, Errors>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var domainErrors = validationResult.ToErrorList();
                return domainErrors;
            }

            var departmentQuery = _context.DepartmentsRead.AsQueryable();

            // Фильтрация по id
            if (query.Dto.DepartmentId != null)
            {
                var queryId = query.Dto.DepartmentId.Value;
                departmentQuery = departmentQuery.Where(x => x.Id == DepartmentId.Current(queryId));
            }

            // Фильтрация по поисковой строке
            if (!string.IsNullOrWhiteSpace(query.Dto.Search))
            {
                var search = query.Dto.Search.Trim();

                var wildcardPattern = $"%{query.Dto.Search.Trim()}%";
                departmentQuery = departmentQuery.Where(l =>
                    NpgsqlDbFunctionsExtensions.ILike(EF.Functions, l.Name.Value, wildcardPattern));
            }

            long totalCount = await EntityFrameworkQueryableExtensions.LongCountAsync(departmentQuery, cancellationToken);

            var sortBy = query.Dto.SortBy?.ToLower().Trim();
            var sortDir = query.Dto.SortDir?.ToLower().Trim();

            // Dynamic Sorting: Includes a default fallback key to prevent pagination instability
            departmentQuery = sortBy switch
            {
                "created" or "createdat" => sortDir == "desc"
                     ? departmentQuery.OrderByDescending(l => l.CreatedAt)
                     : departmentQuery.OrderBy(l => l.CreatedAt),

                // name/null -> отсортируют по имени по возрастанию
                _ => sortDir == "desc"
                    ? departmentQuery.OrderByDescending(l => l.Name.Value)
                    : departmentQuery.OrderBy(l => l.Name.Value)
            };

            int pageSize = query.Dto.PageSize ?? 20;
            int page = query.Dto.Page.HasValue && query.Dto.Page.Value > 0 ? query.Dto.Page.Value : 1;
            int skipCount = (page - 1) * pageSize;

            // Выгружаем саму сущность из БД (EF Core соберет JSON из базы в объекты C#)
            var dbDepartments = await departmentQuery
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync(cancellationToken); // Здесь запрос уходит в Postgres

            // если id не найден
            if (query.Dto.DepartmentId != null && dbDepartments.Count == 0)
            {
                return GeneralErrors.NotFound(query.Dto.DepartmentId.Value, "department.not.found").ToErrors();
            }

            var departments = dbDepartments.Select(x => new DepartmentDto
            {
                Id = x.Id.Value,
                Name = x.Name.Value,
                Identifier = x.Identifier.Value,
                IsActive = x.IsActive,
                Created = x.CreatedAt,
                Updated = x.UpdatedAt,
            }).ToList();

            var response = new DepartmentResponseDto(departments, totalCount);

            return response;
        }
    }
}
