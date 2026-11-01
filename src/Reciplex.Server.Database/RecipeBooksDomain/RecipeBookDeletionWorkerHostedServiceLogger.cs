using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipeBooksDomain;

static partial class RecipeBookDeletionWorkerHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Recipe book deletion worker encountered an exception deleting book '{BookId}'"
    )]
    public static partial void Error_BookDeleteFailed(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        long BookId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Recipe book deletion worker encountered an exception. Pausing work for 5 seconds."
    )]
    public static partial void Error_BackgroundLoopFailure(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "Recipe book deletion worker started")]
    public static partial void Info_RecipeBookLoopStarting(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Recipe book deletion worker stopped")]
    public static partial void Info_RecipeBookLoopStopping(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger
    );
}
