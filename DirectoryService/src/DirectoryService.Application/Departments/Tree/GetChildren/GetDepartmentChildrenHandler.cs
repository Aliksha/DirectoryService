using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments.Tree;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetChildren
{
    public class GetDepartmentChildrenHandler : IQueryHandler<DepartmentChildrenResponseDto, GetDepartmentChildrenQuery>
    {
        private readonly IReadDbContext _context;
        private readonly IValidator<GetDepartmentChildrenQuery> _validator;

        public GetDepartmentChildrenHandler(IReadDbContext context, IValidator<GetDepartmentChildrenQuery> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<DepartmentChildrenResponseDto, Errors>> Handle(GetDepartmentChildrenQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrorList();
            }

            var parentId = DepartmentId.Current(query.ParentId);

            // существование родительского узла
            var parentExists = await _context.DepartmentsRead
                .AsNoTracking()
                .AnyAsync(x => x.Id == parentId, cancellationToken);

            if (!parentExists)
            {
                return GeneralErrors.NotFound(query.ParentId, "department.parent.not.found").ToErrors();
            }

            // выбираем только ПРЯМЫХ детей (ParentId равен текущему)
            // маппинг сразу в SQL через .Select(), исключая N+1
            var childrenQuery = _context.DepartmentsRead
                .AsNoTracking()
                .Where(x => x.ParentId == parentId && x.IsActive)
                .OrderBy(x => x.Name.Value) // сортируем по алфавиту
                .Select(x => new DepartmentTreeItemDto(
                    Id: x.Id.Value,
                    Name: x.Name.Value,
                    Slug: x.Identifier.Value,
                    Path: x.Path.Value,
                    Depth: x.Depth,
                    HasChildren: x.ChildDepartments.Any(), // UI видит, нужно ли рисовать стрелочку раскрытия
                    ChildrenCount: x.ChildDepartments.Count
                ));

            var childrenList = await childrenQuery.ToListAsync(cancellationToken);

            var response = new DepartmentChildrenResponseDto(childrenList);

            return response;
        }
    }
}
