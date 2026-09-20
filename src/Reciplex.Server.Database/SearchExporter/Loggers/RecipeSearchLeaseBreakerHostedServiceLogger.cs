using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchLeaseBreakerHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception while breaking leases")]
    public static partial void Error_LeaseBreakingFailed(
        this ILogger<RecipeSearchLeaseBreakerHostedService> logger,
        Exception ex
    );
}
