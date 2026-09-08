using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexDeletionHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception deleting recipes from the index")]
    public static partial void Error_DeleteRecipesFromIndexFailed(
        this ILogger<RecipeSearchIndexDeletionHostedService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got an exception deleting recipes from the database with too many retries"
    )]
    public static partial void Error_PurgingRecipesFailed(
        this ILogger<RecipeSearchIndexDeletionHostedService> logger,
        Exception ex
    );
}
