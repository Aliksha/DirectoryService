using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Delete
{
    public class DeleteLocationHandler : ICommandHandler<Guid, DeleteLocationCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<DeleteLocationHandler> _logger;

        public DeleteLocationHandler(
            ILocationsRepository locationsRepository,
            ITransactionManager transactionManager,
            ILogger<DeleteLocationHandler> logger)
        {
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken = default)
        {
            var locationId = LocationId.Current(command.Dto.Id);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var checkExistingResult = await _locationsRepository.CheckExisting(new[] { locationId.Value }, cancellationToken);
            if (checkExistingResult.IsFailure)
            {
                return checkExistingResult.Error;
            }

            // 4. Вызываем метод удаления в репозитории локаций
            var deleteResult = await _locationsRepository.DeleteAsync(locationId, cancellationToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogInformation("failed to delete location");
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

            _logger.LogInformation("Location with id {locationId} has been deleted", locationId.Value);

            return locationId.Value;
        }
    }

}
