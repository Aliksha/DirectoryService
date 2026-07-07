using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentDto(
        Guid? DepartmentId,
        string? Search,
        string? SortBy,
        string? SortDir,
        int? Page,
        int? PageSize);
}
