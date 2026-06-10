using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
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

        public async Task<Result<Guid, Error>> AddLocationAsync(Location location, CancellationToken cancellationToken = default)
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
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || ids.Length == 0)
            {
                var emptyError = GeneralErrors.ValueIsRequired("locations.ids.empty");
                return UnitResult.Failure(emptyError.ToErrors());
            }

            var distinctIds = ids.Distinct().ToArray(); // убираем дубликаты
            var domainLocationIds = distinctIds.Select(LocationId.Current).ToList();

            var existingIds = await _context.Locations
                .Where(l => domainLocationIds.Contains(l.Id) && l.IsActive)
                .Select(l => l.Id.Value)
                .ToListAsync(cancellationToken);

            // чего в бд нет
            var missingIds = distinctIds.Except(existingIds).ToList();
            var errorsList = missingIds
                .Select(id => GeneralErrors.NotFound(id, "location"))
                .ToList();

            if (errorsList.Count > 0)
            {
                return UnitResult.Failure(new Errors(errorsList));
            }

            return UnitResult.Success<Errors>();
        }

        public async Task<Result<IReadOnlyCollection<Location>, Errors>> GetLocationsAsync(List<LocationId> ids, CancellationToken cancellationToken = default)
        {
            // защита от лишнего запроса к БД
            if (ids == null || ids.Count == 0)
            {
                IReadOnlyCollection<Location> emptyList = Array.Empty<Location>();
                return Result.Success<IReadOnlyCollection<Location>, Errors>(emptyList);
            }

            try
            {
                var rawIds = ids.Select(id => id.Value).ToList();

                var locations = await _context.LocationsRead
                    .Where(l => rawIds.Contains(l.Id.Value))
                    .ToListAsync(cancellationToken);

                IReadOnlyCollection<Location> resultCollection = locations.AsReadOnly();

                return Result.Success<IReadOnlyCollection<Location>, Errors>(resultCollection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching locations by IDs");

                return Error.Failure("database.error", "Failed to fetch locations from the database").ToErrors();
            }
        }

        public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken) => !await _context.LocationsRead.AnyAsync(x => x.Name.Value == name, cancellationToken);
    }
}
