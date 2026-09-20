using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexExporterStrategyLogger
{
    [LoggerMessage(LogLevel.Error, "Failed to export recipe {RecipeId} because of an exception")]
    public static partial void Error_SearchExportFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Remote rejected recipe {RecipeId}")]
    public static partial void Error_SearchExportFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        long RecipeId
    );

    [LoggerMessage(LogLevel.Error, "Search index exporter lease renewer task threw an exception")]
    public static partial void Error_RenewTaskFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Failed to clear one or more recipe search leases")]
    public static partial void Error_LeaseReleaseFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Failed to mark one or more recipes as extracted")]
    public static partial void Error_MarkingAsExtractedFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed trying to make one or more recipes as failing extraction"
    )]
    public static partial void Error_IncrementingExtractionAttemptFailed(
        this ILogger<RecipeSearchIndexExporterStrategy> logger,
        Exception ex
    );
}
