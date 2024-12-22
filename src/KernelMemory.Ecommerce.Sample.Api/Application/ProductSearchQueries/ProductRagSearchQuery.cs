using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductRagSearchQuery(string SearchQuery) : IQuery<ProductSearchResponse>;

public sealed class ProductRagSearchQueryHandler
    (IKernelMemory memory,
    IOptions<ProductSearchOptions> options) : IQueryHandler<ProductRagSearchQuery, ProductSearchResponse>
{
    public async Task<Result<ProductSearchResponse>> Handle(
        ProductRagSearchQuery request,
        CancellationToken cancellationToken)
    {
        var memoryAnswer = await memory.AskAsync(
            request.SearchQuery,
            minRelevance: options.Value.MinSearchResultsRelevance,
            cancellationToken: cancellationToken);

        if (memoryAnswer.NoResult == true)
        {
            return new ProductSearchResponse(
                memoryAnswer.NoResult,
                options.Value.MinSearchResultsRelevance,
                memoryAnswer.RelevantSources.Count,
                []);
        }

        var foundProducts = JsonSerializer.Deserialize<List<Product>>(memoryAnswer.Result) ?? [];

        return new ProductSearchResponse(
                memoryAnswer.NoResult,
                options.Value.MinSearchResultsRelevance,
                memoryAnswer.RelevantSources.Count,
                foundProducts);
    }
}
