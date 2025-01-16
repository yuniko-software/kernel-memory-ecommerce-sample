using System.ComponentModel.DataAnnotations;

namespace KernelMemory.Ecommerce.Sample.Api.Application.Configuration;

public class ProductSearchOptions
{
    public const string ConfigurationKey = "ProductSearch";

    [Required]
    public required int SearchResultsLimit { get; init; }
    [Required]
    public required double MinSearchResultsRelevance { get; init; }
}