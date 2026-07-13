using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations.ForDapperCase
{
    public record LocationsPagedResponseDto(List<LocationListItemDto> Items, long TotalCount);
}
