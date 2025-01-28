using MediatR;
using Microsoft.KernelMemory;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductStreaming;

public sealed record ProductRagSearchStreamRequest(string SearchQuery) : IStreamRequest<string>;

public class ProductRagSearchStreamingRequestHandler(IKernelMemory memory, IOptions<ProductSearchOptions> options)
    : IStreamRequestHandler<ProductRagSearchStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(
        ProductRagSearchStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var answerStream = memory.AskStreamingAsync(
            request.SearchQuery,
            minRelevance: options.Value.MinSearchResultsRelevance,
            options: new SearchOptions { Stream = true },
            cancellationToken: cancellationToken);

        await foreach (var answer in answerStream)
        {
            yield return answer.Result;
        }
    }
}
