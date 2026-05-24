using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record LocationResponseDto(List<LocationDto> Locations, long TotalCount);
}
