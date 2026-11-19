using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions
{
    public interface ITransactionManager
    {
        Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);

        Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
