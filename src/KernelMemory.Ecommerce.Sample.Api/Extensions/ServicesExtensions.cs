namespace KernelMemory.Ecommerce.Sample.Api.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddEndpoints(assemblies);

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(assemblies));

        return services;
    }
}
