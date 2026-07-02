using Core.Abstractions;
using DirectoryService.Contracts.Positions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Delete
{
    public record DeletePositionCommand(DeletePositionDto Dto) : ICommand;
}
