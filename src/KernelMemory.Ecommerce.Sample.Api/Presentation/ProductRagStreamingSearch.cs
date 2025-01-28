using KernelMemory.Ecommerce.Sample.Api.Application.ProductStreaming;
using MediatR;

namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public class ProductRagStreamingSearch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/search/rag/streaming", async (
            string searchQuery,
            ISender sender,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            response.ContentType = "text/event-stream";

            var request = new ProductRagSearchStreamRequest(searchQuery);

            var resultStream = sender.CreateStream(request, cancellationToken);

            await foreach (var result in resultStream)
            {
                await response.WriteAsync(result, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        });
    }
}
