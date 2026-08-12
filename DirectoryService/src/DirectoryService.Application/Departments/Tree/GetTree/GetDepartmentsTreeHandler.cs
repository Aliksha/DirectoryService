using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments.Tree;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetTree
{
    public class GetDepartmentsTreeHandler : IQueryHandler<DepartmentsTreeResponseDto, GetDepartmentsTreeQuery>
    {
        private readonly IReadDbContext _context;

        public GetDepartmentsTreeHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DepartmentsTreeResponseDto, Errors>> Handle(GetDepartmentsTreeQuery query, CancellationToken cancellationToken = default)
        {
            var rootDepartmentsQuery = _context.DepartmentsRead
                .AsNoTracking()
                .Where(x => x.Depth == 1 && x.IsActive)
                .OrderBy(x => x.Name.Value)
                .Select(x => new DepartmentTreeItemDto(
                    Id: x.Id.Value,
                    Name: x.Name.Value,
                    Slug: x.Identifier.Value,
                    Path: x.Path.Value,
                    Depth: x.Depth,
                    // есть ли подразделения внутри
                    HasChildren: x.ChildDepartments.Any(),
                    ChildrenCount: x.ChildDepartments.Count
                ));

            // один точечный sql запрос в postgres
            var roots = await rootDepartmentsQuery.ToListAsync(cancellationToken);

            var response = new DepartmentsTreeResponseDto(roots);

            return response;
        }
    }
}
