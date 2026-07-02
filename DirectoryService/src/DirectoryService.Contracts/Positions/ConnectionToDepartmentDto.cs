using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Positions
{
    public record ConnectionToDepartmentDto(Guid PositionId, Guid DepartmentId);
}
