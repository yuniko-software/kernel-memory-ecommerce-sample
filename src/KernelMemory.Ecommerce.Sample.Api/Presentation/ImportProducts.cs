using KernelMemory.Ecommerce.Sample.Api.Application;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public sealed class ImportProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/import", async (IFormFile file, ISender sender) =>
        {
            if (file == null || file.Length == 0)
            {
                return ApiResults.Problem(
                    "Endpoints.ImportProducts.Failed",
                    "File is missing or empty");
            }

            using var stream = file.OpenReadStream();

            Result result = await sender.Send(new ImportProductsCommand(stream));

            if (!result.IsSuccess)
            {
                return ApiResults.Problem(result.Error.Code, result.Error.Description);
            }

            return Results.NoContent();
        })
        .DisableAntiforgery();
    }
}
