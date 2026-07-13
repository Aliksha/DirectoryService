using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Get.GetById;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Application.Locations.Get;
using DirectoryService.Application.Locations.Get.GetById;
using DirectoryService.Application.Locations.GetByDapper;
using DirectoryService.Application.Locations.GetTop;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Positions.Delete;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.ForDapperCase;
using DirectoryService.Contracts.Positions;
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

        [HttpGet("dapper")]
        public async Task<EndpointResult<LocationsPagedResponseDto>> GetByDapper(
            [FromQuery] GetLocationsDto dto,
            [FromServices] IQueryHandler<LocationsPagedResponseDto, GetLocationsQuery> handler,
            CancellationToken cancellationToken)
        {
            var query = new GetLocationsQuery(dto);
            return await handler.Handle(query, cancellationToken);
        }

        [HttpGet("{locationId:guid}")]
        public async Task<EndpointResult<LocationByIdResponseDto>> GetById(
            [FromRoute] Guid locationId,
            [FromServices] IQueryHandler<LocationByIdResponseDto, GetByIdLocationQuery> handler,
            CancellationToken cancellationToken)
        {
            var dto = new GetByIdLocationDto(locationId);
            var query = new GetByIdLocationQuery(dto);
            return await handler.Handle(query, cancellationToken);
        }

        [HttpGet("top")]
        public async Task<EndpointResult<TopLocationsResponseDto>> GetTop(
            [FromServices] IQueryHandler<TopLocationsResponseDto, GetTopLocationsQuery> handler,
            CancellationToken cancellationToken,
            [FromQuery] int count = 5)
        {
            var dto = new GetTopLocationsDto(count);
            var query = new GetTopLocationsQuery(dto);
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

        [HttpDelete("{locationId:guid}")]
        public async Task<EndpointResult<Guid>> Delete(
            [FromRoute] Guid locationId,
            [FromServices] ICommandHandler<Guid, DeleteLocationCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new DeleteLocationDto(locationId);
            var command = new DeleteLocationCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }
    }
}
