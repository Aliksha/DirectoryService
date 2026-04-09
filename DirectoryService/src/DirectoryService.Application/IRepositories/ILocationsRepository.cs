using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface ILocationsRepository
    {
        Task<Result<Guid>> AddLocationAsync(Location location, CancellationToken cancellationToken = default);


    }
}
