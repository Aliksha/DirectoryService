using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Positions.Delete
{
    public class DeletePositionHandler : ICommandHandler<Guid, DeletePositionCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<DeletePositionHandler> _logger;

        public DeletePositionHandler(IPositionsRepository positionsRepository, ITransactionManager transactionManager, ILogger<DeletePositionHandler> logger)
        {
            _positionsRepository = positionsRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(DeletePositionCommand command, CancellationToken cancellationToken = default)
        {
            var positionId = PositionId.Current(command.Dto.Id);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var checkExistingResult = await _positionsRepository.CheckExisting(new[] { positionId.Value}, cancellationToken);
            if (checkExistingResult.IsFailure)
            {
                return checkExistingResult.Error;
            }

            var deleteResult = await _positionsRepository.DeleteAsync(positionId, cancellationToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogInformation("failed to delete position");
                return Error.Failure(null, "db problem").ToErrors();
            }

            var saveChangesAsync = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesAsync.IsFailure)
            {
                return saveChangesAsync.Error.ToErrors();
            }

            var commitedResult = transactionScope.Commit();
            if (commitedResult.IsFailure)
            {
                return commitedResult.Error.ToErrors();
            }

            _logger.LogInformation("Position with id {positionId} has been deleted", positionId.Value);

            return positionId.Value;
        }
    }
}
