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
/// <param name="options">options controlling the behavior of this background service</param>
/// <param name="logger">logger for the hosted service</param>
class RecipesThatFailToDeletePurgerHostedService(
    IRecipeSearchExportStatusRepository repository,
    ResiliencePipelineBuilderFactory resiliencePipelineBuilderFactory,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipesThatFailToDeletePurgerHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        ResiliencePipeline pipeline = resiliencePipelineBuilderFactory.BuildDeleteRowPipeline(ex =>
            ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
        );
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PurgeUntilThereAreNoneAsync(pipeline, stoppingToken);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_PurgingRecipesFailed(ex);
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
