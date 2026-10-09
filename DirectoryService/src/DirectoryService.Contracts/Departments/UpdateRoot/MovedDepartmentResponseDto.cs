using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments.UpdateRoot
{
    public record MovedDepartmentResponseDto(Guid DepartmentId, Guid ParentId, string Path, short Depth, DateTime UpdatedAt);
}
