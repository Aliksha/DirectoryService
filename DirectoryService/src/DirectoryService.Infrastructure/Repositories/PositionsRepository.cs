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
    public class PositionsRepository : IPositionsRepository
    {
        private readonly DirectoryServiceDbContext _context;
        private readonly ILogger<PositionsRepository> _logger;

        public PositionsRepository(DirectoryServiceDbContext context, ILogger<PositionsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Add(position);
                //await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Position {Position.Id} registered in memory tracker", position.Id);
                return position.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure registering position {Position.Id} in tracker", position.Id);
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || ids.Length == 0)
            {
                var emptyError = GeneralErrors.ValueIsRequired("positions.ids.empty");
                return UnitResult.Failure(emptyError.ToErrors());
            }

            var distinctIds = ids.Distinct().ToArray(); // убираем дубликаты
            var domainPositionIds = distinctIds.Select(PositionId.Current).ToList();

            var existingIds = await _context.Positions
                .Where(p => domainPositionIds.Contains(p.Id) && p.IsActive)
                .Select(l => l.Id.Value)
                .ToListAsync(cancellationToken);

            // чего в бд нет
            var missingIds = distinctIds.Except(existingIds).ToList();
            var errorsList = missingIds
                .Select(id => GeneralErrors.NotFound(id, "position"))
                .ToList();

            if (errorsList.Count > 0)
            {
                return UnitResult.Failure(new Errors(errorsList));
            }

            return UnitResult.Success<Errors>();
        }

        public async Task<UnitResult<Errors>> DeleteAsync(PositionId positionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deletePosition = await _context.Positions
                    .FirstOrDefaultAsync(x => x.Id == positionId, cancellationToken);
                if (deletePosition == null)
                {
                    return GeneralErrors.NotFound(null, "position.not.found").ToErrors();
                }

                deletePosition.SoftDelete();

                _logger.LogInformation("Position {PositionId} has been deleted", positionId);

                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting position {PositionId}", positionId);
                return GeneralErrors.DataBase().ToErrors();
            }
        }

        public async Task<Result<Position, Error>> GetById(PositionId id, CancellationToken cancellationToken = default)
        {
            var position = await _context.Positions.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

            if (position is null)
                return GeneralErrors.NotFound(id.Value, "position.not.found");

            return position;
        }

        public async Task<Result<Guid, Error>> UpdateAsync(Position position, CancellationToken cancellationToken = default)
        {
            try
            {
                // пометить сущность как измененную (полезно, если объект пришел из другого контекста,
                // а если он уже отслеживается — EF Core просто проигнорирует повторное прикрепление)
                _context.Positions.Update(position);
                _logger.LogInformation("Position {PositionId} update tracked in memory", position.Id.Value);
                return position.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure updating position {Position.Id}", position.Id);
                return GeneralErrors.ValueIsInvalid("position.update.database.error");
            }
        }
    }
}
