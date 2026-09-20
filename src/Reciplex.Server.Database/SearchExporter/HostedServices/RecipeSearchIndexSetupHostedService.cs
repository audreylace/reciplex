using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Runs one time index setup if needed. Otherwise verifies the index is setup.
/// </summary>
/// <param name="searchIndexRepository">repository for search data</param>
/// <param name="recipeIndexCreationCoordinator">coordinator for starting the background work of other index exporters</param>
class RecipeSearchIndexSetupHostedService(
    ISearchIndexRepository searchIndexRepository,
    IRecipeIndexCreationCoordinator recipeIndexCreationCoordinator,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeSearchIndexSetupHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await searchIndexRepository.EnsureRecipeIndexSetupCompleteAsync(stoppingToken))
                {
                    await recipeIndexCreationCoordinator.DeclareIndexSetupAsync();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // shutdown of host
            }
            catch (Exception ex)
            {
                logger.Error_IndexOperationFailed(ex);
            }

            await SafeDelay.DelayAsync(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
