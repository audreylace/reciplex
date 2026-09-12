using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexExporterStrategyLogger
{
    [LoggerMessage(LogLevel.Error, "Renewal loop threw an exception.")]
    public static partial void Error_RenewalLoopFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception exporting new rows to the search index.")]
    public static partial void Error_SearchExportFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );
}
