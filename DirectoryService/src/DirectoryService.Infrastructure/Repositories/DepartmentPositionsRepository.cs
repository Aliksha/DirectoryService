using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
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
    public class DepartmentPositionsRepository : IDepartmentPositionsRepository
    {
        private readonly DirectoryServiceDbContext _context;
        private readonly ILogger<DepartmentPositionsRepository> _logger;

        public DepartmentPositionsRepository(DirectoryServiceDbContext context, ILogger<DepartmentPositionsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> AddConnectionAsync(DepartmentPosition departmentPosition, CancellationToken cancellationToken)
        {
            try
            {
                await _context.DepartmentPositions.AddAsync(departmentPosition, cancellationToken);

                _logger.LogInformation("Connection Department-Position {DepartmentPosition.Id} has been added", departmentPosition.Id);

                return departmentPosition.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure connecting department to position {DepartmentPositionId}", departmentPosition.Id);
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> DeleteConnectionAsync(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken)
        {
            try
            {
                var connectionToDelete = await _context.DepartmentPositions
                    .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId, cancellationToken);
                if(connectionToDelete == null)
                {
                    return GeneralErrors.NotFound(null, "connection.not.found").ToErrors();
                }

                _context.DepartmentPositions.Remove(connectionToDelete);

                _logger.LogInformation("Connection between Department {DepartmentId} and Position {PositionId} has been deleted", departmentId, positionId);

                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting connection between department {DepartmentId} and location {PositionId}", departmentId, positionId);
                return GeneralErrors.DataBase().ToErrors();
            }
        }

        public async Task<bool> IsConnectedAlready(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken = default)
        {
            return await _context.DepartmentPositions
                .AnyAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId, cancellationToken);
        }
    }
}
