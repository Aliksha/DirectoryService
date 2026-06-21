using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface ILocationsRepository
    {
        Task<Result<Guid, Error>> AddLocationAsync(Location location, CancellationToken cancellationToken = default);

        Task<Result<IReadOnlyCollection<Location>, Errors>> GetLocationsAsync(List<LocationId> ids, CancellationToken cancellationToken = default);

        Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken);

        Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default);

        Task<Result<Location, Error>> GetById(LocationId id, CancellationToken cancellationToken = default);

        Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken = default);
    }
}
