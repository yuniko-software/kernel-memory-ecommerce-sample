using Microsoft.KernelMemory;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = BuildAsynchronousKernelMemoryApp(builder);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        await app.RunAsync();
    }

    private static WebApplication BuildAsynchronousKernelMemoryApp(WebApplicationBuilder appBuilder)
    {
        var openAiConfig = new OpenAIConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:OpenAI", openAiConfig);

        var postgresConfig = new PostgresConfig();
        appBuilder.Configuration.BindSection("KernelMemory:Services:Postgres", postgresConfig);

        appBuilder.AddKernelMemory(kmb =>
        {
            kmb.WithOpenAI(openAiConfig);
            kmb.WithPostgresMemoryDb(postgresConfig);
        });

        return appBuilder.Build();
    }
}
