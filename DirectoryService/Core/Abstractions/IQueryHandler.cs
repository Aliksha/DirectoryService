using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions
{
    public interface IQuery;

    public interface IQueryHandler<TResponse, in TQuery>
        where TQuery : IQuery
    {
        Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
    }
}
