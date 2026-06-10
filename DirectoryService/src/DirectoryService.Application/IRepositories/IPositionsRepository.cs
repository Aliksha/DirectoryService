using CSharpFunctionalExtensions;
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
    }
}
