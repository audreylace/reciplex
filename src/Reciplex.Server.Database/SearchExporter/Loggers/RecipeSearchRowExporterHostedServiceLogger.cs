using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchRowExporterHostedServiceLogger
{
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
}
