namespace KernelMemory.Ecommerce.Sample.Api.Domain;

public sealed record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCurrency,
    int SupplyAbility,
    int MinimumOrder);
