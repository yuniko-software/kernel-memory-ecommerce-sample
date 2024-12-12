using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public sealed record ImportProductsCommand(Stream ProductsFileStream) : ICommand;

public sealed class ImportProductsCommandHandler(ICsvReader<ProductCsvModel> csvReader)
    : ICommandHandler<ImportProductsCommand>
{
    public async Task<Result> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        var result = await csvReader.ReadRecordsAsync(request.ProductsFileStream, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        return Result.Success();
    }
}
