using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations.ForDapperCase
{
    public record GetLocationsDto(
        string? Search = null,
        int? MinDepartmentCount = null,
        string? SortBy = "name",
        string? SortDir = "asc",
        int? Page = 1,
        int? PageSize = 20);
}
