using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record LocationGetDto
        (
            Guid[]? DepartmentIds,
            string? Search,
            bool? IsActive,
            string? SortBy,
            int? Page,
            int? PageSize
        );
}
