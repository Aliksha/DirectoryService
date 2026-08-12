using Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetAncestors
{
    public record GetDepartmentAncestorsQuery(Guid ChildId) : IQuery;
}
