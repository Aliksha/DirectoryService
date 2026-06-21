using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record ConnectionToLocationDto(Guid DepartmentId, Guid LocationId);
}
