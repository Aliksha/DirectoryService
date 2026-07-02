using Core.Abstractions;
using DirectoryService.Application.Departments.ConnectToLocation;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Delete;
using DirectoryService.Application.Departments.DisconnectLocation;
using DirectoryService.Application.Departments.Update;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("/api/departments")]
    public class DepartmentController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Post(
            [FromBody] DepartmentCreateDto dto,
            [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new CreateDepartmentCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPatch]
        public async Task<EndpointResult<Guid>> Update(
            [FromBody] UpdateDepartmentDto dto,
            [FromServices] ICommandHandler<Guid, UpdateDepartmentCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{departmentId:guid}")]
        public async Task<EndpointResult<Guid>> Delete(
            [FromRoute] Guid departmentId,
            [FromServices] ICommandHandler<Guid, DeleteDepartmentCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new DeleteDepartmentDto(departmentId);
            var command = new DeleteDepartmentCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPost("{departmentId:guid}/locations/{locationId:guid}")]
        public async Task<EndpointResult<Guid>> ConnectToLocation(
            [FromRoute] Guid departmentId,
            [FromRoute] Guid locationId,
            [FromServices] ICommandHandler<Guid, ConnectToLocationCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new ConnectionToLocationDto(departmentId, locationId);
            var command = new ConnectToLocationCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{departmentId:guid}/locations/{locationId:guid}")]
        public async Task<EndpointResult<Guid>> DisonnectLocation(
            [FromRoute] Guid departmentId,
            [FromRoute] Guid locationId,
            [FromServices] ICommandHandler<Guid, DisconnectLocationCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new ConnectionToLocationDto(departmentId, locationId);
            var command = new DisconnectLocationCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }
    }
}
