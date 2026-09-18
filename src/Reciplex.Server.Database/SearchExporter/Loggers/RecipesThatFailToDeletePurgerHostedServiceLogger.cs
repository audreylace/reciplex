using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipesThatFailToDeletePurgerHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Max failure hit while trying to delete recipes from the database with too many retries"
    )]
    public static partial void Error_PurgingRecipesFailed(
        this ILogger<RecipesThatFailToDeletePurgerHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception while pausing")]
    public static partial void Error_ExceptionDuringPause(
        this ILogger<RecipesThatFailToDeletePurgerHostedService> logger,
        Exception ex
    );
}
