using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments.Tree
{
    public record DepartmentSearchResponseDto(List<DepartmentTreeItemDto> Items);
}
