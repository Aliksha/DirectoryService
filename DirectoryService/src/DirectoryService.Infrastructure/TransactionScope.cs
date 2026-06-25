using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DirectoryService.Infrastructure
{
    public class TransactionScope : ITransactionScope
    {
        private readonly IDbTransaction _transaction;
        private readonly ILogger<TransactionScope> _logger;

        public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
        {
            _transaction = transaction;
            _logger = logger;
        }

        public UnitResult<Error> Commit()
        {
            try
            {
                _transaction.Commit();
                return UnitResult.Success<Error>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to commit transaction");
                return Error.Failure("transaction.commit.failed", "failed to commit transaction");
            }
        }

        public UnitResult<Error> Rollback()
        {
            try
            {
                _transaction.Rollback();
                return UnitResult.Success<Error>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to rollback transaction");
                return Error.Failure("transaction.rollback.failed", "failed to rollback transaction");
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
