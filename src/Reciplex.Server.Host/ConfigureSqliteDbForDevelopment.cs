using Microsoft.Extensions.Options;
using Reciplex.Server.Database;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host;

#if DEBUG

public class SqliteDbDebugSeeding
{
    public const string SectionPath = "Reciplex:Debug:SqliteSeeding";
    public bool Enabled { get; set; }
}

sealed class ConfigureSqliteDbForDevelopment(IServiceProvider rootServices) : IHostedService
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA2201:Do not raise reserved exception types",
        Justification = "Debug only development code"
    )]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
        if (await applicationDbContext.Database.EnsureCreatedAsync(cancellationToken))
        {
            var options = services.GetRequiredService<IOptions<SqliteDbDebugSeeding>>();
            if (options.Value.Enabled)
            {
                var userService = services.GetRequiredService<IUsersService>();
                var userResult = await userService.CreateUserAsync(
                    new()
                    {
                        Subject = "1",
                        Authority = "DEBUG",
                        DisplayName = "Debug User",
                    },
                    cancellationToken
                );

                if (userResult.Result is not SuccessResult<UserDao> userSuccess)
                {
                    throw new Exception();
                }

                var userResult1 = await userService.CreateUserAsync(
                    new()
                    {
                        Subject = "1",
                        Authority = "DEBUG",
                        DisplayName = "Debug User",
                    },
                    cancellationToken
                );

                if (userResult1.Result is not SuccessResult<UserDao> userSuccess1)
                {
                    throw new Exception();
                }

                UserDao userRecord = userSuccess.Value;

                var recipeRepository = services.GetRequiredService<IRecipesService>();
                var booksRepository = services.GetRequiredService<IRecipeBooksService>();
                for (int i = 0; i < 100; i++)
                {
                    var createBookResult = await booksRepository.CreateRecipeBookAsync(
                        userRecord.Id,
                        new() { Name = $"book {i}", ShortDescription = "" },
                        cancellationToken
                    );
                    if (createBookResult.Result is not SuccessResult<RecipeBookDao> bookSuccess)
                    {
                        throw new Exception();
                    }

                    var updateResult = await booksRepository.UpdateShareKeyAsync(
                        bookSuccess.Value.Id,
                        userRecord.Id,
                        bookSuccess.Value.ConcurrencyTag,
                        BookShareKeyUpdateKind.Regenerate,
                        cancellationToken
                    );

                    if (updateResult.Result is not SuccessResult<RecipeBookDao> bookSuccess1)
                    {
                        throw new Exception();
                    }
                    bookSuccess = bookSuccess1;

                    if (i % 6 == 2)
                    {
                        DatabaseResultVariant<
                            SuccessResult<RecipeBookAccessRequestStatus>,
                            NotFoundResult,
                            ValidationFailureResult,
                            ConflictResult,
                            UserNotFoundResult
                        > reqRes = await booksRepository.RequestAccessAsync(
                            bookSuccess.Value.Id,
                            userSuccess1.Value.Id,
                            bookSuccess.Value.ShareKey,
                            cancellationToken
                        );

                        if (reqRes.Result is not SuccessResult<RecipeBookAccessRequestStatus>)
                        {
                            throw new Exception();
                        }

                        DatabaseResultVariant<
                            EmptySuccessResult,
                            NotFoundResult,
                            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
                            ValidationFailureResult,
                            ConflictResult
                        > updateRes = await booksRepository.UpdateUsersAccessAsync(
                            bookSuccess.Value.Id,
                            userSuccess.Value.Id,
                            [
                                new(
                                    userSuccess1.Value.Id,
                                    new()
                                    {
                                        Reviewed = true,
                                        MayEditBook = true,
                                        MayViewBook = true,
                                    }
                                ),
                            ],
                            cancellationToken
                        );

                        if (updateRes.Result is not EmptySuccessResult)
                        {
                            throw new Exception();
                        }
                    }

                    if (i > 10)
                    {
                        continue;
                    }

                    for (int j = 0; j < 100; j++)
                    {
                        var recipeResult = await recipeRepository.CreateRecipeAsync(
                            bookKey: bookSuccess.Value.Id,
                            userKey: userRecord.Id,
                            new()
                            {
                                Name = $"recipe {j}",
                                ShortDescription = "",
                                Details = $"# Recipe {j}\r\n",
                            },
                            cancellationToken
                        );
                        if (recipeResult.Result is not SuccessResult<RecipeDao>)
                        {
                            throw new Exception();
                        }
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
