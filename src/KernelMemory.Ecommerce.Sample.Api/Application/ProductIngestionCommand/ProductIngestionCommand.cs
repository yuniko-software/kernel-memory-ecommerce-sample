using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.CsvReader;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductIngestionCommand;

public sealed record ProductIngestionCommand(Stream ProductsFileStream) : ICommand<IReadOnlyCollection<string>>;

public sealed class ProductIngestionCommandHandler(
    ICsvReader<Product> csvReader, IKernelMemory memory) : ICommandHandler<ProductIngestionCommand, IReadOnlyCollection<string>>
{
    public async Task<Result<IReadOnlyCollection<string>>> Handle(ProductIngestionCommand request, CancellationToken cancellationToken)
    {
        var readingResult = await csvReader.ReadRecordsAsync(request.ProductsFileStream, cancellationToken);
        if (!readingResult.IsSuccess)
        {
            return Result.Failure<IReadOnlyCollection<string>>(readingResult.Error);
        }

        // Note: The GenerateEmbeddingsParallelHandler could potentially be used here as an alternative.
        // However, it is currently experimental and may not yet be stable for production use

        Task<string> __CreateImportTextTask(Product product) =>
            memory.ImportTextAsync(
               JsonSerializer.Serialize(product),
               documentId: product.Id.ToString(),
               cancellationToken: cancellationToken);

        // Need to process one task first to init several things inside KernelMemory.
        // Of the tasks is creating a table in database if it does not exist.
        Task<string> firstImportTask = readingResult.Value.Take(1)
            .Select(__CreateImportTextTask).Single();

        string[] firstDocumentId = await Task.WhenAll(firstImportTask);

        var importTasks = readingResult.Value.Skip(1).Select(__CreateImportTextTask);
        var documentIds = await Task.WhenAll(importTasks);

        var result = new List<string>();
        result.AddRange(firstDocumentId);
        result.AddRange(documentIds);

        return result;
    }
}