using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class NewRecipeSearchRowExporterHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Got an exception waiting for new recipes in the new recipe export loop."
    )]
    public static partial void Error_NewRecipeExportLoopChannelFailed(
        this ILogger<NewRecipeSearchRowExporterHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the new recipe export loop.")]
    public static partial void Error_NewRecipeExportLoopFailed(
        this ILogger<NewRecipeSearchRowExporterHostedService> logger,
        Exception ex
    );
}
