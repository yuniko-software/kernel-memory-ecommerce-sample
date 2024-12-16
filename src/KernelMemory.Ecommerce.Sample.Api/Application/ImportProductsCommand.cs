using KernelMemory.Ecommerce.Sample.Api.Domain;
using Microsoft.KernelMemory;

namespace KernelMemory.Ecommerce.Sample.Api.Application;

public sealed record ImportProductsCommand(string FileName, Stream ProductsFileStream) : ICommand;

public sealed class ImportProductsCommandHandler(IKernelMemory memory) : ICommandHandler<ImportProductsCommand>
{
    public async Task<Result> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        await memory.ImportDocumentAsync(
            request.ProductsFileStream,
            fileName: request.FileName,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
