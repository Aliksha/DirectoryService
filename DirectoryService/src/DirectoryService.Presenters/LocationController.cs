using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("/api/locations")]
    public class LocationController
    {
        [HttpPost]
        public async Task<Result<Guid>> Post(
            [FromBody] LocationCreateDto dto,
            [FromServices] ICommandHandler<Guid, LocationCreateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new LocationCreateCommand(dto);

            return await handler.Handle(command, cancellationToken);
        }
    }
}
