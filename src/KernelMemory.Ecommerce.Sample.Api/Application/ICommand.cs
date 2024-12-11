using KernelMemory.Ecommerce.Sample.Api.Domain;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public interface IBaseCommand;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;
