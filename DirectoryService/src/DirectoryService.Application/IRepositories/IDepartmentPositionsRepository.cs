using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface IDepartmentPositionsRepository
    {
        Task<bool> IsConnectedAlready(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken = default);

        Task<Result<Guid, Error>> AddConnectionAsync(DepartmentPosition departmentPosition, CancellationToken cancellationToken);

        Task<UnitResult<Errors>> DeleteConnectionAsync(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken);
    }
}
