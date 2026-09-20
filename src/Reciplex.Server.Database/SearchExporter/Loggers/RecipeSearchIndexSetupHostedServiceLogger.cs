using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexSetupHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Exception creating or verifying the recipe search index is setup"
    )]
    public static partial void Error_IndexOperationFailed(
        this ILogger<RecipeSearchIndexSetupHostedService> logger,
        Exception? ex
    );
}
