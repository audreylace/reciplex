using Microsoft.Extensions.Options;
using Reciplex.Server.Database;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host;

#if DEBUG

public class SqliteDbDev
{
    public const string SectionPath = "Reciplex:SqliteDebug";
    public bool Enabled { get; set; }
}

class ConfigureSqliteDbForDevelopment(IServiceProvider rootServices) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
        if (await applicationDbContext.Database.EnsureCreatedAsync(cancellationToken))
        {
            var options = services.GetRequiredService<IOptions<SqliteDbDev>>();
            if (options.Value.Enabled)
            {
                var userService = services.GetRequiredService<IUsersRepository>();
                var userResult = await userService.CreateUserAsync(
                    new()
                    {
                        Subject = "1",
                        Authority = "DEBUG",
                        DisplayName = "Debug User",
                    },
                    cancellationToken
                );

                if (userResult is not CreateUserResult.Success userSuccess)
                {
                    throw new Exception();
                }
                UserDao userRecord = userSuccess.User;

                var booksRepository = services.GetRequiredService<IRecipeBooksRepository>();
                for (int i = 0; i < 100; i++)
                {
                    var createBookResult = await booksRepository.CreateRecipeBookAsync(
                        userRecord.Id,
                        new() { Name = $"book {i}", ShortDescription = "" },
                        cancellationToken
                    );
                    if (createBookResult is not CreateRecipeBookResult.Success)
                    {
                        throw new Exception();
                    }
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
#endif
