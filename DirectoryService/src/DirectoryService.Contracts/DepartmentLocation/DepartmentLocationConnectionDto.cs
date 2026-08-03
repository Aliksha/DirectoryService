using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.DepartmentLocation
{
    public record DepartmentLocationConnectionDto(
        string DepartmentName,
        string LocationName,
        AddressDto LocationAddress);
}
