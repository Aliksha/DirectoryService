using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Positions
{
    public record CreatePositionDto(string Name, string? Description, Guid[] Departments);
}
