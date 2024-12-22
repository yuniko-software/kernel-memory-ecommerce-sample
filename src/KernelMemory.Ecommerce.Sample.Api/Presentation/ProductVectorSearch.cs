using KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public class ProductVectorSearch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/search/vector", async (string searchQuery, ISender sender) =>
        {
            var result = await sender.Send(new ProductVectorSearchQuery(searchQuery));

            return Results.Ok(result.Value);
        });
    }
}
