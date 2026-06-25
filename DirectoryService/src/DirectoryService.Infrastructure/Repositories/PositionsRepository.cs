using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
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
    }
}
