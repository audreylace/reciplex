using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Purges recipes from the search index periodically
/// </summary>
/// <param name="sp"></param>
/// <param name="options"></param>
/// <param name="concurrencyTagProvider"></param>
sealed class RecipeSearchIndexDeletionHostedService(
    IServiceProvider sp,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider concurrencyTagProvider,
    ILogger<RecipeSearchIndexDeletionHostedService> logger
) : BackgroundService
{
    const int MaxErrorRetries = 20;
    const int DbDelayInMs = 200; // 200ms delay so the database can breathe. Important for SQLite3 backend.

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        int backOffCounter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var jitter = Random.Shared.Next(30);
            await Task.Delay(
                TimeSpan.FromSeconds(45 + jitter + Math.Pow(2, backOffCounter)),
                stoppingToken
            );
            bool purgedBrokenRecords = await PurgeStuckRecipesAsync(stoppingToken);
            bool deletedRecordsFromIndex = await DeleteRecipesFromIndexAsync(stoppingToken);
            if (purgedBrokenRecords || deletedRecordsFromIndex)
            {
                backOffCounter = 0;
            }
            else
            {
                backOffCounter = Math.Min(backOffCounter + 1, 12);
            }
        }
    }

    private async Task<bool> DeleteRecipesFromIndexAsync(CancellationToken ct)
    {
        bool didWork = false;
        bool hasNetworkProblem = false;
        const int BatchSize = 20;
        try
        {
            while (!ct.IsCancellationRequested && !hasNetworkProblem)
            {
                await using var scope = sp.CreateAsyncScope();

                IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository =
                    scope.ServiceProvider.GetRequiredService<IRecipeSearchExportStatusRepository>();
                ISearchIndexRepository searchIndexRepository =
                    scope.ServiceProvider.GetRequiredService<ISearchIndexRepository>();

                var entries = await recipeSearchExportStatusRepository.GetRecipesToDeleteAsync(
                    BatchSize,
                    MaxErrorRetries,
                    ct
                );
                if (entries.Count < 1)
                {
                    break;
                }
                string leaseToken = concurrencyTagProvider.NextTag();
                if (
                    await recipeSearchExportStatusRepository.ClaimAsync(
                        entries,
                        leaseToken,
                        5 * 60,
                        ct
                    ) > 0
                )
                {
                    entries = await recipeSearchExportStatusRepository.GetClaimedRecipes(
                        leaseToken,
                        ct
                    );

                    if (entries.Count > 0)
                    {
                        var outcome = await searchIndexRepository.DeleteRecipesAsync(entries, ct);
                        if (outcome == IndexMutationOperationOutcome.Error) // network problem - stop looping
                        {
                            hasNetworkProblem = true;
                            break;
                        }
                        else if (outcome == IndexMutationOperationOutcome.BatchFailed)
                        {
                            foreach (var entry in entries)
                            {
                                outcome = await searchIndexRepository.DeleteRecipesAsync(
                                    [entry],
                                    ct
                                );
                                if (outcome == IndexMutationOperationOutcome.Error) // network problem - stop looping
                                {
                                    hasNetworkProblem = true;
                                    break;
                                }
                                else if (outcome == IndexMutationOperationOutcome.BatchFailed)
                                {
                                    await recipeSearchExportStatusRepository.MarkRecipeDeletionFailedAndReleaseAsync(
                                        entry,
                                        leaseToken,
                                        ct
                                    );
                                }
                                else if (outcome == IndexMutationOperationOutcome.Success)
                                {
                                    didWork = true;
                                    await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                                        [entry],
                                        leaseToken,
                                        ct
                                    );
                                }
                            }
                        }
                        else if (outcome == IndexMutationOperationOutcome.Success)
                        {
                            didWork = true;
                            await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                                entries,
                                leaseToken,
                                ct
                            );
                        }
                    }
                }
                await Task.Delay(DbDelayInMs, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_DeleteRecipesFromIndexFailed(ex);
        }

        return didWork;
    }

    private async Task<bool> PurgeStuckRecipesAsync(CancellationToken ct)
    {
        bool anyWork = false;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await using var scope = sp.CreateAsyncScope();
                IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository =
                    scope.ServiceProvider.GetRequiredService<IRecipeSearchExportStatusRepository>();
                ISearchIndexRepository searchIndexRepository =
                    scope.ServiceProvider.GetRequiredService<ISearchIndexRepository>();

                var entries =
                    await recipeSearchExportStatusRepository.PurgeRecipeSearchEntriesWithTooManyRetries(
                        100,
                        MaxErrorRetries,
                        ct
                    );
                if (entries < 1)
                {
                    break;
                }
                anyWork = true;
                await Task.Delay(DbDelayInMs, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_PurgingRecipesFailed(ex);
        }

        return anyWork;
    }
}
