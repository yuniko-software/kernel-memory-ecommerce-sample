using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using KernelMemory.Ecommerce.Sample.FunctionalTests.Abstractions;
using Microsoft.KernelMemory;
using NSubstitute;

namespace KernelMemory.Ecommerce.Sample.FunctionalTests;

public class ProductIngestionTests : IClassFixture<TestWebAppFactory>
{
    private const string Endpoint = "api/products/ingestion";

    private readonly HttpClient _httpClient;
    private readonly IKernelMemory _mockKernelMemory;

    public ProductIngestionTests(TestWebAppFactory factory)
    {
        _httpClient = factory.CreateClient();
        _mockKernelMemory = factory.MockKernelMemory;
    }

    [Fact]
    public async Task ProductIngestion_WithCsvFileHavingWrongFormat_ReturnsBadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "invalid-products.csv");
        using var fileStream = File.OpenRead(filePath);
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        // Act
        var response = await _httpClient.PostAsync(Endpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProductIngestion_WithInvalidFile_ReturnsBadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();

        using var textContent = new StringContent("Sample text content", Encoding.UTF8, "text/plain");
        content.Add(textContent, "file", "sample.txt");

        // Act
        var response = await _httpClient.PostAsync(Endpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProductIngestion_WithValidFile_ReturnsOkResultWithDocumentIds()
    {
        // Arrange
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "valid-products.csv");

        var productIds = await ReadProductIdsFromCsv(filePath);
        foreach (var productId in productIds)
        {
            _mockKernelMemory.ImportTextAsync(Arg.Any<string>(), Arg.Is<string>(x => x == productId))
                .Returns(productId);
        }

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        // Act
        var response = await _httpClient.PostAsync(Endpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var documentIds = await response.Content.ReadFromJsonAsync<List<string>>();

        Assert.NotNull(documentIds);
        Assert.NotEmpty(documentIds);
        Assert.Equal(productIds.Count, documentIds.Count);
        Assert.All(productIds, productId => Assert.Contains(productId, documentIds));
    }

    private async Task<List<string>> ReadProductIdsFromCsv(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            HasHeaderRecord = true,
        });

        var records = await csv.GetRecordsAsync<Product>().ToListAsync();
        return records.Select(x => x.Id.ToString()).ToList();
    }
}
