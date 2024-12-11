namespace KernelMemory.Ecommerce.Sample.Api.Application.ImportProducts;

public sealed record ImportProductsCommand(Stream ProductsFileStream) : ICommand;
