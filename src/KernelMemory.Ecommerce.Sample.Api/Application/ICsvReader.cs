using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public interface ICsvReader<TRecord>
{
    Task<Result<IReadOnlyCollection<TRecord>>> ReadRecordsAsync(
        Stream stream, CancellationToken cancellationToken = default);
}
