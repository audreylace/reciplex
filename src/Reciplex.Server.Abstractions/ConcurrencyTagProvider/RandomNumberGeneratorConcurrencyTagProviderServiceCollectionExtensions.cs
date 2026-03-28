using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Reciplex.Server.Abstractions.ConcurrencyTagProvider;

public static class RandomNumberGeneratorConcurrencyTagProviderServiceCollectionExtensions
{
    public static IServiceCollection AddRandomNumberGeneratorConcurrencyTagProvider(
        this IServiceCollection services
    )
    {
        services.TryAddSingleton<
            IConcurrencyTagProvider,
            RandomNumberGeneratorConcurrencyTagProvider
        >();
        return services;
    }
}
