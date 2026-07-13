using Core.Abstractions;
using DirectoryService.Contracts.Locations.ForDapperCase;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.GetByDapper
{
    public record GetLocationsQuery(GetLocationsDto Dto) : IQuery;
}
