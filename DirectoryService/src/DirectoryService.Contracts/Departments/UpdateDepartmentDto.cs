using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record UpdateDepartmentDto(Guid Id, string? Name, string? Identifier, Guid[]? LocationsId);
}
