using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations
{
    public record LocationCreateCommand(LocationCreateDto Dto) : ICommand;
}
