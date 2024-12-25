using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Infrastructure;
using KernelMemory.Ecommerce.Sample.Api.Presentation;
using KernelMemory.Ecommerce.Sample.Api.Infrastructure.Logging;
using Microsoft.KernelMemory;
using Serilog;

namespace KernelMemory.Ecommerce.Sample.Api;

public sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, loggerConfig) => 
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddApplicationServices();

        builder.AddOpenTelemetry();

        var app = BuildAsynchronousKernelMemoryApp(builder);

        app.UseDefaultFiles();
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging();

        app.MapEndpoints();

        await app.RunAsync();
    }

    private static WebApplication BuildAsynchronousKernelMemoryApp(WebApplicationBuilder appBuilder)
    {
        var openAiConfig = new OpenAIConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:OpenAI", openAiConfig);

        var qdrantConfig = new QdrantConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:Qdrant", qdrantConfig);

        // Uncomment and configure this section if you want to use Postgres as the memory database
        //var postgresConfig = new PostgresConfig();
        //appBuilder.Configuration.BindSection("KernelMemory:Services:Postgres", postgresConfig);

        var searchClientConfig = new SearchClientConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Retrieval:SearchClient", searchClientConfig);

        var productSearchOptions = appBuilder.Configuration
            .GetRequiredSection(ProductSearchOptions.ConfigurationKey)
            .Get<ProductSearchOptions>()
            ?? throw new InvalidProgramException(ProductSearchOptions.ConfigurationKey);

        appBuilder.AddKernelMemory(kmb =>
        {
            kmb.WithOpenAI(openAiConfig);
            kmb.WithQdrantMemoryDb(qdrantConfig);
            // Uncomment the following line to enable Postgres as a memory database.
            //kmb.WithPostgresMemoryDb(postgresConfig);
            kmb.WithSearchClientConfig(searchClientConfig);

            kmb.WithCustomPromptProvider(new ProductSearchPromptProvider(productSearchOptions.SearchResultsLimit));
        });

        return appBuilder.Build();
    }
}
