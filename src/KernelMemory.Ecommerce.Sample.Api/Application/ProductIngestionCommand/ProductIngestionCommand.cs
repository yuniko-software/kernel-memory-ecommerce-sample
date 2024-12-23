using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.CsvReader;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductIngestionCommand;

public sealed record ProductIngestionCommand(Stream ProductsFileStream) : ICommand;

public sealed class ProductIngestionCommandHandler(
    ICsvReader<Product> csvReader, IKernelMemory memory) : ICommandHandler<ProductIngestionCommand>
{
    public async Task<Result> Handle(ProductIngestionCommand request, CancellationToken cancellationToken)
    {
        var readingResult = await csvReader.ReadRecordsAsync(request.ProductsFileStream, cancellationToken);
        if (!readingResult.IsSuccess)
        {
            return Result.Failure(readingResult.Error);
        }

        // Note: The GenerateEmbeddingsParallelHandler could potentially be used here as an alternative.
        // However, it is currently experimental and may not yet be stable for production use
        var importTasks = readingResult.Value.Select(product =>
            memory.ImportTextAsync(
                JsonSerializer.Serialize(product),
                documentId: product.Id.ToString(),
                cancellationToken: cancellationToken)).ToArray();

        await Task.WhenAll(importTasks);

        return Result.Success();
    }
}
