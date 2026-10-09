using Core.Abstractions;
using DirectoryService.Contracts.Departments.UpdateRoot;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.MoveRoot
{
    public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentDto Dto) : ICommand;
}
