using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;

namespace Reciplex.Server.SearchExporter.HostedServices;

/// <summary>
/// Deletes recipe search status entries that are pending delete when
/// the search feature is off.
/// </summary>
/// <param name="options">application options for search</param>
/// <param name="recipeSearchExportStatusRepository">search status repository</param>
/// <param name="logger">application logger</param>
sealed class PurgeMarkedForDeletionHostedService(
    IOptions<SearchExporterOptions> options,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ILogger<PurgeMarkedForDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.Enable)
        {
            return;
        }

        logger.Info_Starting();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await recipeSearchExportStatusRepository.WaitForSearchDeleteWorkAsync(
                    TimeSpan.FromMinutes(1),
                    stoppingToken
                );
                await recipeSearchExportStatusRepository.PurgeMarkedForDeletionAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error_GotException(ex);
            }
        }

        logger.Info_Stopping();
    }
}
