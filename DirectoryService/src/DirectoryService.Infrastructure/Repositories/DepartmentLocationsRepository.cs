using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Repositories
{
    public class DepartmentLocationsRepository : IDepartmentLocationsRepository
    {
        private readonly DirectoryServiceDbContext _context;
        private readonly ILogger<DepartmentLocationsRepository> _logger;

        public DepartmentLocationsRepository(DirectoryServiceDbContext context, ILogger<DepartmentLocationsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> AddConnectionAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken)
        {
            try
            {
                await _context.DepartmentLocations.AddAsync(departmentLocation, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Connection Department-Location {DepartmentLocation.Id} has been added", departmentLocation.Id);

                return departmentLocation.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure connecting department to location {DepartmentLocationId}", departmentLocation.Id);
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> DeleteConnectionAsync(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken)
        {
            try
            {
                var connectionToDelete = await _context.DepartmentLocations
                    .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.LocationId == locationId, cancellationToken);
                if (connectionToDelete == null)
                {
                    return GeneralErrors.NotFound(null, "connection.not.found").ToErrors();
                }

                _context.DepartmentLocations.Remove(connectionToDelete);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Connection between Department {DepartmentId} and Location {LocationId} has been deleted", departmentId, locationId);

                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting connection between department {DepartmentId} and location {LocationId}", departmentId, locationId);
                return GeneralErrors.DataBase().ToErrors();
            }
        }

        public async Task<bool> IsConnectedAlready(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken = default)
        {
            // проверка по условию WHERE
            return await _context.DepartmentLocations
                .AnyAsync(x => x.DepartmentId == departmentId && x.LocationId == locationId, cancellationToken);
        }
    }
}
