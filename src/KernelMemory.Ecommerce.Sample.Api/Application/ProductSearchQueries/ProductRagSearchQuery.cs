using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductRagSearchQuery(string SearchQuery) : IQuery<ProductSearchResponse>;

public sealed class ProductRagSearchQueryHandler(
    IKernelMemory memory,
    IOptions<ProductSearchOptions> options,
    ILogger<ProductRagSearchQueryHandler> logger) : IQueryHandler<ProductRagSearchQuery, ProductSearchResponse>
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
            return ProductSearchResponse.NoProducts(options.Value.MinSearchResultsRelevance);
        }

        List<Product> foundProducts;
        try
        {
            foundProducts = JsonSerializer.Deserialize<List<Product>>(memoryAnswer.Result) ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize memoryAnswer.Result for query '{Query}'", request.SearchQuery);

            return ProductSearchResponse.NoProducts(options.Value.MinSearchResultsRelevance);
        }

        return new ProductSearchResponse(
                memoryAnswer.NoResult,
                options.Value.MinSearchResultsRelevance,
                memoryAnswer.RelevantSources.Count,
                foundProducts);
    }
}
