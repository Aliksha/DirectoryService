using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
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
                _context.Locations.Add(location);

                _logger.LogInformation("Location {Location.Id} registered in memory tracker", location.Id);

                return location.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure registering location {Location.Id} in tracker", location.Id);
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

        public async Task<UnitResult<Errors>> DeleteAsync(LocationId locationId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deleteLocation = await _context.Locations
                    .FirstOrDefaultAsync(x => x.Id == locationId, cancellationToken);
                if(deleteLocation == null)
                {
                    return GeneralErrors.NotFound(null, "location.not.found").ToErrors();
                }

                _context.Locations.Remove(deleteLocation);

                _logger.LogInformation("Location {LocationId} has been deleted", locationId);

                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting location {LocationId}", locationId);
                return GeneralErrors.DataBase().ToErrors();
            }
        }

        public async Task<Result<Location, Error>> GetById(LocationId id, CancellationToken cancellationToken = default)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

            if (location is null)
                return GeneralErrors.NotFound(id.Value, "location.not.found");

            return location;
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

        public async Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            try
            {
                // пометить сущность как измененную (полезно, если объект пришел из другого контекста,
                // а если он уже отслеживается — EF Core просто проигнорирует повторное прикрепление)
                _context.Locations.Update(location);
                _logger.LogInformation("Location {LocationId} update tracked in memory", location.Id.Value);
                return location.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure updating location {Location.Id}", location.Id);
                return GeneralErrors.ValueIsInvalid("location.update.database.error");
            }
        }
    }
}
