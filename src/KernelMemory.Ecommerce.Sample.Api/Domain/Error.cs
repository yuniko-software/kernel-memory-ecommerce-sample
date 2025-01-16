namespace KernelMemory.Ecommerce.Sample.Api.Domain;

public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("General.Null", "Null value was provided");
}