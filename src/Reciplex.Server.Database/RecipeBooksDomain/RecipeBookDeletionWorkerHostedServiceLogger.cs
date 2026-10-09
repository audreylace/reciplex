using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipeBooksDomain;

static partial class RecipeBookDeletionWorkerHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception deleting book '{BookId}'")]
    public static partial void Error_BookDeleteFailed(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        long BookId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the delete book loop")]
    public static partial void Error_BackgroundLoopFailure(
        this ILogger<RecipeBookDeletionWorkerHostedService> logger,
        Exception ex
    );
}
