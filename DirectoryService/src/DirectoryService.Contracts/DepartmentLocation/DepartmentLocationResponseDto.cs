using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.DepartmentLocation
{
    public record DepartmentLocationResponseDto(List<DepartmentLocationConnectionDto> DepartmentLocationConnections);
}
