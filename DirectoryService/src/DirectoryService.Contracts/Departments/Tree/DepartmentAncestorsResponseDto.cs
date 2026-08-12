using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments.Tree
{
    public record DepartmentAncestorsResponseDto(List<DepartmentTreeItemDto> Ancestors);
}
