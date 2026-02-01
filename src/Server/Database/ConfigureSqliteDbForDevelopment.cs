using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Extensions;
using Reciplex.Server.Database;
using Reciplex.Server.Database.DbObjects;

namespace Recipe.Database;

#if DEBUG
class ConfigureSqliteDbForDevelopment(IServiceProvider rootServices, IClock clock) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
        if (await applicationDbContext.Database.EnsureCreatedAsync(cancellationToken))
        {
            await SeedDataAsync(applicationDbContext, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SeedDataAsync(
        ApplicationDbContext applicationDbContext,
        CancellationToken cancellationToken
    )
    {
        UserDbObject testUser = new() { DisplayName = "Testing User 1", Id = 1 };
        applicationDbContext.Add(testUser);

        RecipeBookDbObject bakeBook = new()
        {
            Name = "Baking",
            ShortDescription = "Banking goods and other yummy recipes",
        };
        RecipeBookDbObject texMex = new()
        {
            Name = "TextMex Explosion",
            ShortDescription = "Cheese, Salt, Hot Sauce, all the yummies",
        };
        RecipeBookDbObject mixDrinks = new()
        {
            Name = "Mixed Drinks",
            ShortDescription = "Drinks that will make your knees wobble and your insides warm",
        };

        mixDrinks.Recipes.Add(
            new()
            {
                Name = "Liquor",
                ShortDescription = "Alcoholic liquor",
                LastModified = clock.GetCurrentInstant().ToUnixTimeSeconds(),
                Created = clock.GetCurrentInstant().ToUnixTimeSeconds(),
                ConcurrencyTag = "abcd",
            }
        );

        for (int i = 0; i < 40; i++)
        {
            RecipeDbObject fakeRecipe = new()
            {
                Name = "Fake Recipe " + i,
                ShortDescription = "Fake recipe number " + i,
                LastModified = clock.GetCurrentInstant().ToUnixTimeSeconds(),
                Created = clock.GetCurrentInstant().ToUnixTimeSeconds(),
                ConcurrencyTag = "abcd",
            };
            mixDrinks.Recipes.Add(fakeRecipe);
        }

        testUser.RecipeBooks.Add(bakeBook);
        testUser.RecipeBooks.Add(texMex);
        testUser.RecipeBooks.Add(mixDrinks);

        for (int i = 0; i < 40; i++)
        {
            RecipeBookDbObject fakeRecipe = new()
            {
                Name = "Fake Recipe " + i,
                ShortDescription = "Fake recipe number " + i,
            };
            testUser.RecipeBooks.Add(fakeRecipe);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
#endif
