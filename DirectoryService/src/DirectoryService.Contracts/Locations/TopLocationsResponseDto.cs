using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record TopLocationsResponseDto(List<TopLocationDto> TopLocations);
}
