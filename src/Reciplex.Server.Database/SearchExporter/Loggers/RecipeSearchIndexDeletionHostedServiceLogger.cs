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

    [LoggerMessage(LogLevel.Error, "Got an exception deleting recipe {RecipeId} from the index")]
    public static partial void Error_DeletingRecipeFromIndexFailed(
        this ILogger<RecipeSearchIndexDeletionHostedService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception waiting for the renew task")]
    public static partial void Error_RenewTaskFailed(
        this ILogger<RecipeSearchIndexDeletionHostedService> logger,
        Exception ex
    );
}
