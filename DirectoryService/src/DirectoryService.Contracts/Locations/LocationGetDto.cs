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


    //public record GetLocationDto
    //{
    //    public Guid[]? DepartmentIds { get; init; }
    //    public string? Search { get; init; }
    //    public bool? IsActive { get; init; }
    //    public string? SortBy { get; init; }

    //    private readonly int? _page;
    //    public int? Page
    //    {
    //        get => _page;
    //        init => _page = value is <= 0 ? 1 : value; // защита от отрицательных страниц
    //    }

    //    private readonly int? _pageSize;
    //    public int? PageSize
    //    {
    //        get => _pageSize;
    //        // если клиент хочет больше 100, принудительно урезаем до 100
    //        init => _pageSize = value is > 100 ? 100 : (value is <= 0 ? 20 : value);
    //    }
    //}

}
