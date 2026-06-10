using Core.Abstractions;
using DirectoryService.Contracts.Departments;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Create
{
    public record CreateDepartmentCommand(DepartmentCreateDto Dto) : ICommand;
}
