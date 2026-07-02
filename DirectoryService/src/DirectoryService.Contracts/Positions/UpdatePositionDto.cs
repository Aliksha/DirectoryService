using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Positions
{
    public record UpdatePositionDto(
        Guid Id,
        string? Name,
        string? Description,
        Guid[]? DepartmentsId);
}
