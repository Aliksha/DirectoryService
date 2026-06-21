using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Get;
using DirectoryService.Application.Locations.Update;
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

        [HttpGet]
        public async Task<EndpointResult<LocationResponseDto>> Get(
            [FromQuery] LocationGetDto dto, // данные фильтрации, пагинации и сортировки из Query String
            [FromServices] IQueryHandler<LocationResponseDto, LocationsGetQuery> handler,
            CancellationToken cancellationToken)
        {
            var query = new LocationsGetQuery(dto);

            return await handler.Handle(query, cancellationToken);
        }

        [HttpPatch]
        public async Task<EndpointResult<Guid>> Update(
            [FromBody] UpdateLocationDto dto,
            [FromServices] ICommandHandler<Guid, UpdateLocationCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateLocationCommand(dto);

            return await handler.Handle(command, cancellationToken);
        }
    }
}
