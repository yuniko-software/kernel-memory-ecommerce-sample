using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using KernelMemory.Ecommerce.Sample.FunctionalTests.Abstractions;
using Microsoft.KernelMemory;
using NSubstitute;

namespace KernelMemory.Ecommerce.Sample.FunctionalTests;

public class ProductVectorSearchTests : IClassFixture<TestWebAppFactory>
{
    private const string Endpoint = "api/products/search/vector";

    private readonly HttpClient _httpClient;
    private readonly IKernelMemory _mockKernelMemory;
    private readonly ProductSearchOptions _options;

    public ProductVectorSearchTests(TestWebAppFactory factory)
    {
        _httpClient = factory.CreateClient();
        _mockKernelMemory = factory.MockKernelMemory;

        _options = new ProductSearchOptions() { MinSearchResultsRelevance = 0.8, SearchResultsLimit = 5 };

        factory.ProductSearchOptions.Value.Returns(_options);
    }

    [Fact]
    public async Task ProductVectorSearch_WithNoResultResponseFromKernel_ReturnsOkResultWithNoProducts()
    {
        // Arrange
        var searchQuery = "some weird query";
        var url = Endpoint + $"?searchQuery={searchQuery.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var limit = _options.SearchResultsLimit;
        var searchResult = new SearchResult();

        _mockKernelMemory.SearchAsync(
            Arg.Is<string>(query => query == searchQuery),
            minRelevance: Arg.Is<double>(relevance => relevance == minRelevance),
            limit: Arg.Is<int>(lim => lim == limit),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(searchResult);

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
    public async Task ProductVectorSearch_WithNonSerializableResponseFromKernel_ReturnsOkResultWithNoProducts()
    {
        // Arrange
        var searchQuery = "Give me gaming consoles";
        var url = Endpoint + $"?searchQuery={searchQuery.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var limit = _options.SearchResultsLimit;
        var searchResult = new SearchResult()
        {
            Results = [new Citation() { Partitions = [new Citation.Partition() { Text = "Xbox series X" }] }]
        };

        _mockKernelMemory.SearchAsync(
            Arg.Is<string>(query => query == searchQuery),
            minRelevance: Arg.Is<double>(relevance => relevance == minRelevance),
            limit: Arg.Is<int>(lim => lim == limit),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(searchResult);

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
    public async Task ProductRagSearch_WithCorrectResponseFromKernel_ReturnsOkResultWithProducts()
    {
        // Arrange
        var searchQuery = "Give me gaming consoles";
        var url = Endpoint + $"?searchQuery={searchQuery.Replace(' ', '+')}";

        var minRelevance = _options.MinSearchResultsRelevance;
        var limit = _options.SearchResultsLimit;
        var product = new Product(Guid.NewGuid(), "Xbox Series X", "Gaming console", 500, "USD", 2, 1);
        var partitionToSearch = new Citation.Partition() { Text = JsonSerializer.Serialize(product) };
        var searchResult = new SearchResult()
        {
            Results = [new Citation() { Partitions = [partitionToSearch] }]
        };

        _mockKernelMemory.SearchAsync(
            Arg.Is<string>(query => query == searchQuery),
            minRelevance: Arg.Is<double>(relevance => relevance == minRelevance),
            limit: Arg.Is<int>(lim => lim == limit),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(searchResult);

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