using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public sealed record ImportProductsCommand(Stream ProductsFileStream) : ICommand;

public sealed class ImportProductsCommandHandler(IKernelMemory memory) : ICommandHandler<ImportProductsCommand>
{
    public async Task<Result> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        await memory.ImportDocumentAsync(
            request.ProductsFileStream,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
