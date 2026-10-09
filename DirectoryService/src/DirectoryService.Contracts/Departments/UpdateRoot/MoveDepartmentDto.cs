using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments.UpdateRoot
{
    public record MoveDepartmentDto(Guid? ParentId);
}
