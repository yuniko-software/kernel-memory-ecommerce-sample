using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductSearchResponse(
    bool NoResult,
    double MinRelevance,
    int RelevantSourcesCount,
    IReadOnlyCollection<Product> Products)
{
    public static ProductSearchResponse NoProducts(double minRelevance)
    {
        return new ProductSearchResponse(
            NoResult: true,
            MinRelevance: minRelevance,
            RelevantSourcesCount: 0,
            Products: []);
    }
}
