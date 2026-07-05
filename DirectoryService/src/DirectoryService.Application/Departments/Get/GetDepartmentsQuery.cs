using Core.Abstractions;
using DirectoryService.Contracts.Departments;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Get
{
    public record GetDepartmentsQuery(GetDepartmentDto Dto) : IQuery;
}
