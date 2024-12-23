using KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public class ProductRagSearch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/search/rag", async (string searchQuery, ISender sender) =>
        {
            var result = await sender.Send(new ProductRagSearchQuery(searchQuery));

            return Results.Ok(result.Value);
        });
    }
}
