using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchRowExporterHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception exporting new rows to the search index.")]
    public static partial void Error_SearchExportFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the new recipe export loop.")]
    public static partial void Error_NewRecipeExportLoopFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got an exception waiting for new recipes in the new recipe export loop."
    )]
    public static partial void Error_NewRecipeExportLoopChannelFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got an exception waiting for recipe change notifications in the changed recipe export loop."
    )]
    public static partial void Error_ChangedRecipeExportLoopChannelFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the change recipe export loop.")]
    public static partial void Error_ChangedRecipeExportLoopFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Renewal loop threw an exception.")]
    public static partial void Error_RenewalLoopFailed(
        this ILogger<RecipeSearchRowExporterHostedService> logger,
        Exception ex
    );
}
