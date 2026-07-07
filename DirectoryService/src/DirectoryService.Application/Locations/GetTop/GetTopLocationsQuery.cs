using Core.Abstractions;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.GetTop
{
    public record GetTopLocationsQuery(GetTopLocationsDto Dto) : IQuery;
}
