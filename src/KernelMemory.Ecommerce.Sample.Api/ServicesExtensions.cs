using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Application.CsvReader;
using KernelMemory.Ecommerce.Sample.Api.Domain;
using KernelMemory.Ecommerce.Sample.Api.Infrastructure;
using KernelMemory.Ecommerce.Sample.Api.Presentation;

namespace KernelMemory.Ecommerce.Sample.Api;

public static class ServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddEndpoints(assemblies);

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(assemblies));

        services.AddSingleton(typeof(ICsvReader<Product>), typeof(CsvReader<Product>));

        services.AddOptions<ProductSearchOptions>()
            .BindConfiguration(ProductSearchOptions.ConfigurationKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}