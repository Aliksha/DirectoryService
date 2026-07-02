using Core.Abstractions;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Delete
{
    public record DeleteLocationCommand(DeleteLocationDto Dto) : ICommand;
}
