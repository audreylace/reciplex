using NodaTime;
using Reciplex.Server.Database;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.Recipes;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host;

#if DEBUG
class ConfigureSqliteDbForDevelopment(IServiceProvider rootServices) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
        if (await applicationDbContext.Database.EnsureCreatedAsync(cancellationToken))
        {
            await SeedDataAsync(
                applicationDbContext,
                services.GetRequiredService<IRecipeBookService>(),
                services.GetRequiredService<IRecipeService>(),
                cancellationToken
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task SeedDataAsync(
        ApplicationDbContext applicationDbContext,
        IRecipeBookService bookService,
        IRecipeService recipeService,
        CancellationToken cancellationToken
    )
    {
        UserDbObject testUser = new() { DisplayName = "Testing User 1", Id = 1 };
        applicationDbContext.Add(testUser);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        UserKey userKey = new(testUser.Id);
        (
            await bookService.CreateRecipeBookAsync(
                userKey,
                new()
                {
                    Name = "Baking (Empty Book)",
                    ShortDescription = "Banking goods and other yummy recipes",
                },
                cancellationToken
            )
        ).EnsureSuccess();

        (
            await bookService.CreateRecipeBookAsync(
                userKey,
                new()
                {
                    Name = "TextMex Explosion (Empty Book)",
                    ShortDescription = "Cheese, Salt, Hot Sauce, all the yummies",
                },
                cancellationToken
            )
        ).EnsureSuccess();

        var mixedBooks = await bookService.CreateRecipeBookAsync(
            userKey,
            new()
            {
                Name = "Mixed Drinks (40 recipes)",
                ShortDescription = "Drinks that will make your knees wobble and your insides warm",
            },
            cancellationToken
        );
        mixedBooks.EnsureSuccess();

        for (int i = 0; i < 40; i++)
        {
            var recipe = await recipeService.CreateRecipeAsync(
                mixedBooks.Book.Id,
                userKey,
                new()
                {
                    Name = "Liquor Drink " + i,
                    ShortDescription = "Alcoholic liquor iteration " + i,
                    Details = "",
                },
                cancellationToken
            );
            recipe.EnsureSuccess();
        }

        for (int i = 0; i < 40; i++)
        {
            (
                await bookService.CreateRecipeBookAsync(
                    userKey,
                    new() { Name = "Empty book " + i, ShortDescription = "Description" },
                    cancellationToken
                )
            ).EnsureSuccess();
        }
    }
}
#endif
