using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly DirectoryServiceDbContext _context;
        private readonly ILogger<LocationsRepository> _logger;

        public LocationsRepository(DirectoryServiceDbContext context, ILogger<LocationsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddLocationAsync(Location location, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Locations.AddAsync(location, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Location {Location.Id} created", location.Id);

                return location.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure adding location {Location.Id}", location.Id);
                return Result.Failure<Guid>("Failure adding location ");
            }
        }
    }
}
