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

namespace DirectoryService.Application.Departments.Get.GetById
{
    public class GetByIdDepartmentHandler : IQueryHandler<DepartmentByIdResponseDto, GetByIdDepartmentQuery>
    {
        public readonly IReadDbContext _context;

        public GetByIdDepartmentHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DepartmentByIdResponseDto, Errors>> Handle(GetByIdDepartmentQuery query, CancellationToken cancellationToken = default)
        {
            var departmentId = DepartmentId.Current(query.Dto.DepartmentId);

            var department = await _context.DepartmentsRead
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);

            if (department == null)
            {
                return GeneralErrors.NotFound(query.Dto.DepartmentId, "department.not.found").ToErrors();
            }

            var departmentDto = new DepartmentDto
            {
                Id = department.Id.Value,
                Name = department.Name.Value,
                Identifier = department.Identifier.Value,
                IsActive = department.IsActive,
                Created = department.CreatedAt,
                Updated = department.UpdatedAt
            };

            var responseDto = new DepartmentByIdResponseDto(departmentDto);
            return Result.Success<DepartmentByIdResponseDto, Errors>(responseDto);
        }
    }
}
