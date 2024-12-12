namespace KernelMemory.Ecommerce.Sample.Api.Application;

public sealed record ProductCsvModel(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCurrency,
    int SupplyAbility,
    int MinimumOrder
);
