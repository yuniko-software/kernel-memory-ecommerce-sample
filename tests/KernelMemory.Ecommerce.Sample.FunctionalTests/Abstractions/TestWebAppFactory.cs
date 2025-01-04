using KernelMemory.Ecommerce.Sample.Api;
using KernelMemory.Ecommerce.Sample.Api.Application.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using NSubstitute;

namespace KernelMemory.Ecommerce.Sample.FunctionalTests.Abstractions;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    public IKernelMemory MockKernelMemory { get; } = Substitute.For<IKernelMemory>();
    public IOptions<ProductSearchOptions> ProductSearchOptions { get; } = Substitute.For<IOptions<ProductSearchOptions>>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(MockKernelMemory);
            services.AddSingleton(ProductSearchOptions);
        });
    }
}
