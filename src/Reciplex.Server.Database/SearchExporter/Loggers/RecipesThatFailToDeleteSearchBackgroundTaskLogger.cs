using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipesThatFailToDeleteSearchBackgroundTaskLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Max failure hit while trying to delete recipes from the database with too many retries"
    )]
    public static partial void Error_PurgingRecipesFailed(
        this ILogger<RecipesThatFailToDeleteSearchBackgroundTask> logger,
        Exception ex
    );
}
