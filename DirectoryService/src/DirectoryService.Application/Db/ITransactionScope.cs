using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Db
{
    public interface ITransactionScope : IDisposable
    {
        UnitResult<Error> Commit();

        UnitResult<Error> Rollback();
    }
}
