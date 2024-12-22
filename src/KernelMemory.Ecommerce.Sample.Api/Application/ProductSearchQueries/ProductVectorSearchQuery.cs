using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductVectorSearchQuery(string SearchQuery) : IQuery<ProductSearchResponse>;

public sealed class ProductVectorSearchQueryHandler(
    IKernelMemory memory,
    IOptions<ProductSearchOptions> options) : IQueryHandler<ProductVectorSearchQuery, ProductSearchResponse>
{
    public async Task<Result<ProductSearchResponse>> Handle(
        ProductVectorSearchQuery request,
        CancellationToken cancellationToken)
    {
        var memoryAnswer = await memory.SearchAsync(
            request.SearchQuery,
            minRelevance: options.Value.MinSearchResultsRelevance,
            limit: options.Value.SearchResultsLimit,
            cancellationToken: cancellationToken);

        if (memoryAnswer.NoResult == true)
        {
            return new ProductSearchResponse(
                memoryAnswer.NoResult,
                options.Value.MinSearchResultsRelevance,
                memoryAnswer.Results.Count,
                []);
        }

        var foundProducts = memoryAnswer.Results
            .SelectMany(res => res.Partitions)
            .Select(part => JsonSerializer.Deserialize<Product>(part.Text)!)
            .ToList();

        return new ProductSearchResponse(
            memoryAnswer.NoResult,
            options.Value.MinSearchResultsRelevance,
            memoryAnswer.Results.Count,
            foundProducts);
    }
}
