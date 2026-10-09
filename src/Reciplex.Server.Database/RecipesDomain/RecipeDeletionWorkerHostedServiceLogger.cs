using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.RecipesDomain;

static partial class RecipeDeletionWorkerHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception deleting recipes")]
    public static partial void Error_RecipeDeletionLoopFailed(
        this ILogger<RecipeDeletionWorkerHostedService> logger,
        Exception ex
    );
}
