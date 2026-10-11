using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipesDomain;

static partial class RecipePurgeWorkerHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Got an exception deleting recipes. Pausing deletion loop for 5 seconds."
    )]
    public static partial void Error_RecipeDeletionLoopFailed(
        this ILogger<RecipePurgeWorkerHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "Recipe purge worker hosted service started")]
    public static partial void Info_StartingRecipePurgeWorker(
        this ILogger<RecipePurgeWorkerHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Recipe purge worker hosted service stopping")]
    public static partial void Info_StoppingRecipePurgeWorker(
        this ILogger<RecipePurgeWorkerHostedService> logger
    );
}
