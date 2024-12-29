using KernelMemory.Ecommerce.Sample.Api.Application.ProductIngestionCommand;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public sealed class ProductIngestion : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products/ingestion", async (IFormFile file, ISender sender) =>
        {
            if (file == null || file.Length == 0)
            {
                return ApiResults.Problem(
                    "Endpoints.ProductsIngestion.Failed",
                    "File is missing or empty");
            }

            using var stream = file.OpenReadStream();

            Result<IReadOnlyCollection<string>> result = await sender.Send(new ProductIngestionCommand(stream));

            if (!result.IsSuccess)
            {
                return ApiResults.Problem(result.Error.Code, result.Error.Description);
            }

            return Results.Ok(result.Value);
        })
        .DisableAntiforgery();
    }
}
