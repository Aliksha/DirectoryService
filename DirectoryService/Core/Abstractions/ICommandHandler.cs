using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions
{
    public interface ICommand;

    public interface ICommandHandler<TResponse, in TCommand>
        where TCommand : ICommand
    {
        Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
