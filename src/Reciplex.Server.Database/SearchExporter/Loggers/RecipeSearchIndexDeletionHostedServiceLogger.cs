using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexDeletionHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Unhandled exception caught in recipe search index deletion background service"
    )]
    public static partial void Error_DeleteRecipesFromIndexFailed(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception deleting recipe {RecipeId} from the index")]
    public static partial void Error_DeletingRecipeFromIndexFailed(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Delete from search index failed for {RecipeId}")]
    public static partial void Error_DeletingRecipeFromIndexFailed(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        long RecipeId
    );

    [LoggerMessage(LogLevel.Error, "Search index deletion lease renewer task threw an exception")]
    public static partial void Error_RenewTaskFailed(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Failed to clear one or more recipe search row leases")]
    public static partial void Error_FailedToClearLeases(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to clear leases and delete one or more recipe search row entries after purging their data from the search index"
    )]
    public static partial void Error_FailedToDeleteRows(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment failure counter and clear leases for one or more recipe search row entries after attempting to purging their data from the search index"
    )]
    public static partial void Error_FailedToIncrementRowsFailureCounter(
        this ILogger<RecipeDeletionSearchBackgroundTask> logger,
        Exception ex
    );
}
