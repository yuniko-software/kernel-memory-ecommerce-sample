using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;

namespace KernelMemory.Qdrant.EnhancedClient;

public static class KernelMemoryBuilderExtensions
{
    public static IKernelMemoryBuilder WithEnhancedQdrantClient(this IKernelMemoryBuilder builder)
    {
        builder.Services.AddSingleton<IMemoryDb, EnhancedQdrantMemory>();

        return builder;
    }
}
