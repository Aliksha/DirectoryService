using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly DirectoryServiceDbContext _context;

        public LocationsRepository(DirectoryServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> AddLocationAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _context.AddAsync(location, cancellationToken);

            await _context.SaveChangesAsync();

            return Result.Success(location.Id.Value);
        }
    }
}
