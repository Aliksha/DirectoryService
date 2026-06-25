using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DirectoryService.Infrastructure
{
    public class TransactionManager : ITransactionManager
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<TransactionManager> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public TransactionManager(DirectoryServiceDbContext dbContext, ILogger<TransactionManager> logger, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                var transactionScopeLogger = _loggerFactory.CreateLogger<TransactionScope>();
                var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);
                return transactionScope;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to begin transaction");
                return Error.Failure("database", "failed to begin transaction");
            }
        }

        public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return UnitResult.Success<Error>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database transaction save changes failed");
                Error domainError = DbErrorMapper.Map(ex);
                return UnitResult.Failure(domainError);
            }
        }
    }
}
