using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface IPositionsRepository
    {
        Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default);

        Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default);

        Task<Result<Position, Error>> GetById(PositionId positionId, CancellationToken cancellationToken = default);

        Task<Result<Guid, Error>> UpdateAsync(Position position, CancellationToken cancellationToken = default);

        Task<UnitResult<Errors>> DeleteAsync(PositionId positionId, CancellationToken cancellationToken = default);
    }
}
