using Core.Abstractions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Positions;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("/api/positions")]
    public class PositionController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Post(
            [FromBody] CreatePositionDto dto,
            [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new CreatePositionCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }
    }
}
