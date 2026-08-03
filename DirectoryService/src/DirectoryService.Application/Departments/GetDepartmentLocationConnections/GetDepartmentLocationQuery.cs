using Core.Abstractions;
using DirectoryService.Contracts.DepartmentLocation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.GetDepartmentLocationConnections
{
    public record GetDepartmentLocationQuery(GetDepartmentLocationDto Dto) : IQuery;
}
