using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipesDomain;

static partial class RecipeDeletionHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Got an exception deleting recipes. Pausing work for 5 seconds."
    )]
    public static partial void Error_RecipeDeletionLoopFailed(
        this ILogger<RecipeDeletionHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "Recipe deletion worker started")]
    public static partial void Info_StartingRecipePurgeWorker(
        this ILogger<RecipeDeletionHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Recipe deletion worker stopped")]
    public static partial void Info_StoppingRecipePurgeWorker(
        this ILogger<RecipeDeletionHostedService> logger
    );
}
