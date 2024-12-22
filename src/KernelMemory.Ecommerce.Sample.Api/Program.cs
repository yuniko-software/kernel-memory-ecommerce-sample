using KernelMemory.Ecommerce.Sample.Api;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Infrastructure;
using KernelMemory.Ecommerce.Sample.Api.Presentation;
using Microsoft.KernelMemory;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddApplicationServices();

        var app = BuildAsynchronousKernelMemoryApp(builder);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapEndpoints();

        await app.RunAsync();
    }

    private static WebApplication BuildAsynchronousKernelMemoryApp(WebApplicationBuilder appBuilder)
    {
        var openAiConfig = new OpenAIConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:OpenAI", openAiConfig);

        var postgresConfig = new PostgresConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:Postgres", postgresConfig);

        var searchClientConfig = new SearchClientConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Retrieval:SearchClient", searchClientConfig);

        var productSearchOptions = appBuilder.Configuration
            .GetRequiredSection(ProductSearchOptions.ConfigurationKey)
            .Get<ProductSearchOptions>()
            ?? throw new InvalidProgramException(ProductSearchOptions.ConfigurationKey);

        appBuilder.AddKernelMemory(kmb =>
        {
            kmb.WithOpenAI(openAiConfig);
            kmb.WithPostgresMemoryDb(postgresConfig);
            kmb.WithSearchClientConfig(searchClientConfig);

            kmb.WithCustomPromptProvider(new ProductSearchPromptProvider(productSearchOptions.SearchResultsLimit));
        });

        return appBuilder.Build();
    }
}
