using KernelMemory.Ecommerce.Sample.Api.Domain;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public interface IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
