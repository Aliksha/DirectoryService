using CSharpFunctionalExtensions;
using SharedKernel;
using System.Data;

namespace Core.Abstractions
{
    public interface ITransactionScope : IDisposable
    {
        UnitResult<Error> Commit();

        UnitResult<Error> Rollback();

        public IDbTransaction Transaction { get; }

    }
}
