using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments.Tree
{
    public record DepartmentTreeItemDto(
        Guid Id,
        string Name,
        string Slug, // identifier
        string Path,
        int Depth,
        bool HasChildren,
        int ChildrenCount);
}
