using Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetSearch
{
    public record SearchDepartmentsTreeQuery(string Q) : IQuery;
}
