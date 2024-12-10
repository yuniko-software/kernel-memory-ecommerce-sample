namespace KernelMemory.Ecommerce.Sample.Api.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Brand { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public int SupplyAbility { get; private set; }
    public int MinimumOrder { get; private set; }

    public Product(
        Guid id,
        string name,
        string description,
        Money price,
        int supplyAbility,
        int minimumOrder)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        SupplyAbility = supplyAbility;
        MinimumOrder = minimumOrder;
    }
}
