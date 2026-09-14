using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipesThatFailToDeletePurgerHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Got an exception deleting recipes from the database with too many retries. Running retry attempt {Attempt}."
    )]
    public static partial void Warning_PurgingRecipesFailedRunningRetry(
        this ILogger<RecipesThatFailToDeletePurgerHostedService> logger,
        int Attempt,
        Exception? ex
    );

    [LoggerMessage(
        LogLevel.Warning,
        "Max failure hit while trying to delete recipes from the database with too many retries"
    )]
    public static partial void Error_PurgingRecipesFailed(
        this ILogger<RecipesThatFailToDeletePurgerHostedService> logger,
        Exception ex
    );
}
