using Core.Abstractions;
using DirectoryService.Application.Positions.ConnectToDepartment;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Positions.Delete;
using DirectoryService.Application.Positions.DisconnectDepartment;
using DirectoryService.Application.Positions.Update;
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

        [HttpPatch]
        public async Task<EndpointResult<Guid>> Update(
            [FromBody] UpdatePositionDto dto,
            [FromServices] ICommandHandler<Guid, UpdatePositionCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdatePositionCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{positionId:guid}")]
        public async Task<EndpointResult<Guid>> Delete(
            [FromRoute] Guid positionId,
            [FromServices] ICommandHandler<Guid, DeletePositionCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new DeletePositionDto(positionId);
            var command = new DeletePositionCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPost("{positionId:guid}/departments/{departmentId:guid}")]
        public async Task<EndpointResult<Guid>> ConnectToDepartment(
            [FromRoute] Guid positionId,
            [FromRoute] Guid departmentId,
            [FromServices] ICommandHandler<Guid, ConnectToDepartmentCommand> handler,
            CancellationToken cancellationToken)
        {
            var dto = new ConnectionToDepartmentDto(positionId, departmentId);
            var command = new ConnectToDepartmentCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{positionId:guid}/departments/{departmentId:guid}")]
        public async Task<EndpointResult<Guid>> DisconnectDepartment(
           [FromRoute] Guid positionId,
           [FromRoute] Guid departmentId,
           [FromServices] ICommandHandler<Guid, DisconnectDepartmentCommand> handler,
           CancellationToken cancellationToken)
        {
            var dto = new ConnectionToDepartmentDto(positionId, departmentId);
            var command = new DisconnectDepartmentCommand(dto);
            return await handler.Handle(command, cancellationToken);
        }
    }
}
