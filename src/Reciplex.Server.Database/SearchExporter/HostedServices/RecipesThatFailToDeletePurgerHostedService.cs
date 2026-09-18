using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Background service that purges recipes with too many
/// delete attempts.
/// </summary>
/// <param name="repository">repository managing recipe search rows</param>
/// <param name="resilienceFactory">resilience pipeline builder</param>
/// <param name="options">options controlling the behavior of this background service</param>
/// <param name="logger">logger for the hosted service</param>
class RecipesThatFailToDeletePurgerHostedService(
    IRecipeSearchExportStatusRepository repository,
    ResiliencePipelineBuilderFactory resilienceFactory,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipesThatFailToDeletePurgerHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable) // exit if search is not enabled
        {
            return;
        }

        ResiliencePipeline pipeline = resilienceFactory.BuildDeleteRowPipeline(ex =>
            ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
        );
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeUntilThereAreNoneAsync(pipeline, stoppingToken);
                if (!await periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    return; // exit on shutdown
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit on shutdown
            }
            catch (Exception ex)
            {
                logger.Error_PurgingRecipesFailed(ex);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return; // exit background service
                }
                catch (Exception innerEx)
                {
                    logger.Error_ExceptionDuringPause(innerEx);
                }
            }
        }
    }

    private async Task PurgeUntilThereAreNoneAsync(
        ResiliencePipeline pipeline,
        CancellationToken stoppingToken
    )
    {
        while (
            !stoppingToken.IsCancellationRequested
            && await pipeline.ExecuteAsync(
                async token =>
                    await repository.PurgeRecipeSearchEntriesWithTooManyRetries(
                        options.Value.RecipesFailedToDeletePurgeSize,
                        options.Value.MaxRecipeDeleteAttempts,
                        token
                    ),
                stoppingToken
            ) > 0
        ) { }
    }
}
