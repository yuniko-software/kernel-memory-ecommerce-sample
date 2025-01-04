using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using KernelMemory.Ecommerce.Sample.FunctionalTests.Abstractions;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Context;
using NSubstitute;

namespace KernelMemory.Ecommerce.Sample.FunctionalTests;

public class ProductRagSearchTests : IClassFixture<TestWebAppFactory>
{
    private const string Endpoint = "api/products/search/rag";

    private readonly HttpClient _httpClient;
    // Mocking AskStreamingAsync because AskAsync is an extension method that internally uses AskStreamingAsync.
    // This allows us to intercept the behavior of AskAsync by setting up AskStreamingAsync instead
    private readonly IKernelMemory _mockKernelMemory;
    private readonly ProductSearchOptions _options;

    public ProductRagSearchTests(TestWebAppFactory factory)
    {
        _httpClient = factory.CreateClient();
        _mockKernelMemory = factory.MockKernelMemory;

        _options = new ProductSearchOptions() { MinSearchResultsRelevance = 0.8, SearchResultsLimit = 5 };

        factory.ProductSearchOptions.Value.Returns(_options);
    }

    [Fact]
    public async Task ProductRagSearch_WithNoResultResponseFromLlm_ReturnsOkResultWithNoProducts()
    {
        // Arrange
        var userQuestion = "some weird question";
        var url = Endpoint + $"?searchQuery={userQuestion.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var memoryResult = new[] { new MemoryAnswer() }.ToAsyncEnumerable();

        _mockKernelMemory.AskStreamingAsync(
            Arg.Is<string>(question => question == userQuestion),
            Arg.Any<string?>(),
            Arg.Any<MemoryFilter?>(),
            Arg.Any<ICollection<MemoryFilter>?>(),
            Arg.Is<double>(relevance => relevance == minRelevance),
            Arg.Any<SearchOptions?>(),
            Arg.Any<IContext?>(),
            Arg.Any<CancellationToken>()).Returns(memoryResult);

        // Act
        var httpResponse = await _httpClient.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var searchResponse = await httpResponse.Content.ReadFromJsonAsync<ProductSearchResponse>();

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.NoResult);
        Assert.Equal(minRelevance, searchResponse.MinRelevance);
        Assert.Equal(0, searchResponse.RelevantSourcesCount);
        Assert.Empty(searchResponse.Products);
    }

    [Fact]
    public async Task ProductRagSearch_WithNonSerializableResponseFromLlm_ReturnsOkResultWithNoProducts()
    {
        // Arrange
        var userQuestion = "Give me gaming consoles";
        var url = Endpoint + $"?searchQuery={userQuestion.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var memoryAnswer = new MemoryAnswer() { NoResult = false, Result = "Xbox series X" };
        var memoryAnswers = new[] { memoryAnswer }.ToAsyncEnumerable();

        _mockKernelMemory.AskStreamingAsync(
            Arg.Is<string>(question => question == userQuestion),
            Arg.Any<string?>(),
            Arg.Any<MemoryFilter?>(),
            Arg.Any<ICollection<MemoryFilter>?>(),
            Arg.Is<double>(relevance => relevance == minRelevance),
            Arg.Any<SearchOptions?>(),
            Arg.Any<IContext?>(),
            Arg.Any<CancellationToken>()).Returns(memoryAnswers);

        // Act
        var httpResponse = await _httpClient.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var searchResponse = await httpResponse.Content.ReadFromJsonAsync<ProductSearchResponse>();

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.NoResult);
        Assert.Equal(minRelevance, searchResponse.MinRelevance);
        Assert.Equal(0, searchResponse.RelevantSourcesCount);
        Assert.Empty(searchResponse.Products);
    }

    [Fact]
    public async Task ProductRagSearch_WithCorrectResponseFromLlm_ReturnsOkResultWithProducts()
    {
        // Arrange
        var userQuestion = "Give me gaming consoles";
        var url = Endpoint + $"?searchQuery={userQuestion.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var product = new Product(Guid.NewGuid(), "Xbox Series X", "Gaming console", 500, "USD", 2, 1);
        var productsToReturn = new List<Product>() { product };
        var memoryAnswer = new MemoryAnswer()
        {
            NoResult = false,
            Result = JsonSerializer.Serialize(productsToReturn)
        };
        var memoryAnswers = new[] { memoryAnswer }.ToAsyncEnumerable();

        _mockKernelMemory.AskStreamingAsync(
            Arg.Is<string>(question => question == userQuestion),
            Arg.Any<string?>(),
            Arg.Any<MemoryFilter?>(),
            Arg.Any<ICollection<MemoryFilter>?>(),
            Arg.Is<double>(relevance => relevance == minRelevance),
            Arg.Any<SearchOptions?>(),
            Arg.Any<IContext?>(),
            Arg.Any<CancellationToken>()).Returns(memoryAnswers);

        // Act
        var httpResponse = await _httpClient.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var searchResponse = await httpResponse.Content.ReadFromJsonAsync<ProductSearchResponse>();

        Assert.NotNull(searchResponse);
        Assert.False(searchResponse.NoResult);
        Assert.Equal(minRelevance, searchResponse.MinRelevance);
        Assert.NotEmpty(searchResponse.Products);
        Assert.Contains(product, searchResponse.Products);
    }
}
