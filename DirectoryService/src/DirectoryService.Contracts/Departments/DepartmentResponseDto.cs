using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentResponseDto(List<DepartmentDto> Departments, long TotalCount);
}
