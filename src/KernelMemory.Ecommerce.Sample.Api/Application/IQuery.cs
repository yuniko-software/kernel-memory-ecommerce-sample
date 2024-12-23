using KernelMemory.Ecommerce.Sample.Api.Domain;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

