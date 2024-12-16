namespace KernelMemory.Ecommerce.Sample.Api.Infrastructure;

public sealed record ProductCsvModel(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCurrency,
    int SupplyAbility,
    int MinimumOrder
)
{
    public override string ToString()
    {
        return $"""
            Guid: {Id} 
            Name: {Name} 
            Description: {Description} 
            PriceAmount: {Price} 
            PriceCurrency: {PriceCurrency} 
            SupplyAblility: {SupplyAbility} 
            MinimumOrder: {MinimumOrder} 
            """;
    }
};
