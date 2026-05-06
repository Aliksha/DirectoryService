using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions
{
    public interface ICommand;

    public interface ICommandHandler<TResponse, in TCommand>
        where TCommand : ICommand
    {
        Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
