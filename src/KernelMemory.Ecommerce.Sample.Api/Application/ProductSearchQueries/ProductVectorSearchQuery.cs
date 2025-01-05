using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductVectorSearchQuery(string SearchQuery) : IQuery<ProductSearchResponse>;

public sealed class ProductVectorSearchQueryHandler(
    IKernelMemory memory,
    IOptions<ProductSearchOptions> options,
    ILogger<ProductVectorSearchQueryHandler> logger) : IQueryHandler<ProductVectorSearchQuery, ProductSearchResponse>
{
    public async Task<Result<ProductSearchResponse>> Handle(
        ProductVectorSearchQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await memory.SearchAsync(
            request.SearchQuery,
            minRelevance: options.Value.MinSearchResultsRelevance,
            limit: options.Value.SearchResultsLimit,
            cancellationToken: cancellationToken);

        if (searchResult.NoResult == true)
        {
            return ProductSearchResponse.NoProducts(options.Value.MinSearchResultsRelevance);
        }

        List<Product> foundProducts;
        try
        {
            foundProducts = searchResult.Results
                .SelectMany(res => res.Partitions)
                .Select(part => JsonSerializer.Deserialize<Product>(part.Text)!)
                .ToList();
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize search result partition text for query '{Query}'", request.SearchQuery);

            return ProductSearchResponse.NoProducts(options.Value.MinSearchResultsRelevance);
        }

        return new ProductSearchResponse(
            searchResult.NoResult,
            options.Value.MinSearchResultsRelevance,
            searchResult.Results.Count,
            foundProducts);
    }
}
