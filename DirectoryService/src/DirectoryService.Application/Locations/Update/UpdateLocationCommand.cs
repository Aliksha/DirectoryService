using Core.Abstractions;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Update
{
    public record UpdateLocationCommand(UpdateLocationDto Dto) : ICommand;
}
