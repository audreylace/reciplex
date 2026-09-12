using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipesThatFailToDeletePurgerHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Got an exception deleting recipes from the database with too many retries"
    )]
    public static partial void Error_PurgingRecipesFailed(
        this ILogger<RecipesThatFailToDeletePurgerHostedService> logger,
        Exception ex
    );
}
