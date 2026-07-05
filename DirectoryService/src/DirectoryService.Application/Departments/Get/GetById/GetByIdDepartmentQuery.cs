using Core.Abstractions;
using DirectoryService.Contracts.Departments;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Get.GetById
{
    public record GetByIdDepartmentQuery(GetByIdDepartmentDto Dto) : IQuery;
}
