using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.Departments.Delete
{
    public class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<DeleteLocationHandler> _logger;

        public DeleteDepartmentHandler(
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            ILogger<DeleteLocationHandler> logger)
        {
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken = default)
        {
            var departmentId = DepartmentId.Current(command.Dto.Id);

            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            using var transactionScope = transactionScopeResult.Value;

            var checkExistingResult = await _departmentsRepository.CheckExisting(new[] { departmentId.Value }, cancellationToken);
            if (checkExistingResult.IsFailure)
            {
                return checkExistingResult.Error;
            }

            // 4. Вызываем метод удаления в репозитории локаций
            var deleteResult = await _departmentsRepository.DeleteAsync(departmentId, cancellationToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogInformation("failed to delete department");
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

            _logger.LogInformation("Location with id {departmentId} has been deleted", departmentId.Value);

            return departmentId.Value;
        }
    }
}
