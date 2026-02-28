using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.Recipes;
using Reciplex.Server.RecipeServices.Utils;

namespace Reciplex.Server.RecipeServices;

public static class RecipeServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRecipeServices(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IRecipeBookService, RecipeBookService>();
        services.AddSingleton<
            IConcurrencyTagProvider,
            RandomNumberGeneratorConcurrencyTagProvider
        >();
        return services;
    }
}
