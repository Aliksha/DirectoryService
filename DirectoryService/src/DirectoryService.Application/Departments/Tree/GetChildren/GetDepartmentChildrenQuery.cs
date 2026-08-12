using Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetChildren
{
    public record GetDepartmentChildrenQuery(Guid ParentId) : IQuery;
}
