using KernelMemory.Ecommerce.Sample.Api.Application;
using KernelMemory.Ecommerce.Sample.Api.Infrastructure;

namespace KernelMemory.Ecommerce.Sample.Api.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddEndpoints(assemblies);

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(assemblies));

        services.AddSingleton(typeof(ICsvReader<ProductCsvModel>), typeof(CsvReader<ProductCsvModel>));

        return services;
    }
}
