using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
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

        public GetDepartmentsHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DepartmentResponseDto, Errors>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken = default)
        {
            var departmentQuery = _context.DepartmentsRead.AsQueryable();

            if(query.Dto.DepartmentId != null)
            {
                var queryId = query.Dto.DepartmentId.Value;
                departmentQuery = departmentQuery.Where(x => x.Id == DepartmentId.Current(queryId));
            }

            if (!string.IsNullOrWhiteSpace(query.Dto.Search))
            {
                var search = query.Dto.Search.Trim();

                var wildcardPattern = $"%{query.Dto.Search.Trim()}%";
                departmentQuery = departmentQuery.Where(l =>
                    NpgsqlDbFunctionsExtensions.ILike(EF.Functions, l.Name.Value, wildcardPattern));
            }

            long totalCount = await EntityFrameworkQueryableExtensions.LongCountAsync(departmentQuery, cancellationToken);

            // Выгружаем саму сущность из БД (EF Core соберет JSON из базы в объекты C#)
            var dbDepartments = await departmentQuery.ToListAsync(cancellationToken); // Здесь запрос уходит в Postgres

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
