using Core.Abstractions;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Create
{
    public record LocationCreateCommand(LocationCreateDto Dto) : ICommand;
}
