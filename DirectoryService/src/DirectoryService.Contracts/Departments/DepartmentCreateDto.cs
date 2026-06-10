using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentCreateDto(string Name, string Identifier, Guid? ParentId, Guid[] LocationsId);
}
