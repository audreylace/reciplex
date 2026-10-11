using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipeBooksDomain;

static partial class RecipeBookDeletionWorkerHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception deleting book '{BookId}'")]
    public static partial void Error_BookDeleteFailed(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        string BookId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the delete book loop")]
    public static partial void Error_BackgroundLoopFailure(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "Starting recipe book deletion book loop")]
    public static partial void Info_RecipeBookLoopStarting(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Stopping recipe book deletion book loop")]
    public static partial void Info_RecipeBookLoopStopping(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger
    );
}
