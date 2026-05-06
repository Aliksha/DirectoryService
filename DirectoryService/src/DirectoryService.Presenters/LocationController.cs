using Core.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("/api/locations")]
    public class LocationController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Post(
            [FromBody] LocationCreateDto dto,
            [FromServices] ICommandHandler<Guid, LocationCreateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new LocationCreateCommand(dto);

            return await handler.Handle(command, cancellationToken);
        }
    }
}
