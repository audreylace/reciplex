using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter;

class RecipeSearchIndexExporterStrategy(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository,
    LeaseRenewer leaseRenewer,
    ILogger<RecipeSearchIndexExporterStrategy> logger
)
{
    public async Task ExportRecipesAsync(List<long> ids, string leaseToken, CancellationToken ct)
    {
        try
        {
            var data = await recipeSearchExportStatusRepository.ExtractRecipeDataAsync(
                ids,
                leaseToken,
                ct
            );
            if (data.Count < 1)
            {
                return;
            }

            using CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(ct);
            var renewerTask = leaseRenewer.ExecuteAsync(
                leaseToken,
                [.. data.Select(d => d.RecipeFk)],
                TimeSpan.FromMinutes(5), // keep lease for 5 minutes
                TimeSpan.FromSeconds(30), // renew every 30 seconds
                8, // attempt renew 8 times
                cancellationTokenSource.Token
            );
            try
            {
                var result = await searchIndexRepository.UpsertRecipesAsync(
                    new()
                    {
                        Recipes =
                        [
                            .. data.Select(d => new RecipeSearchIndexInformation()
                            {
                                RecipeId = d.RecipeFk,
                                RecipeBookId = d.RecipeBookFk,
                                Name = d.Name,
                                ShortDescription = d.ShortDescription,
                            }),
                        ],
                    },
                    ct
                );

                switch (result)
                {
                    case IndexMutationOperationOutcome.Error:
                        break;
                    case IndexMutationOperationOutcome.Success:
                        foreach (var recipe in data)
                        {
                            await recipeSearchExportStatusRepository.MarkRecipeAsExtractedAndReleaseAsync(
                                recipe.RecipeFk,
                                recipe.SearchVersion,
                                leaseToken,
                                ct
                            );
                        }
                        break;
                    case IndexMutationOperationOutcome.BatchFailed:
                        await ExtractRecipesInSerial(leaseToken, data, renewerTask, ct);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                await cancellationTokenSource.CancelAsync();
                try
                {
                    await renewerTask;
                }
                catch (Exception ex)
                {
                    logger.Error_RenewalLoopFailed(ex);
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException
                || ex is OperationCanceledException && !ct.IsCancellationRequested
            )
        {
            logger.Error_SearchExportFailed(ex);
        }
    }

    private async Task ExtractRecipesInSerial(
        string leaseToken,
        List<RecipeRecordDataExtractedFromDatabase> data,
        Task renewerTask,
        CancellationToken ct
    )
    {
        foreach (var recipe in data)
        {
            if (renewerTask.IsCompleted)
            {
                return;
            }

            IndexMutationOperationOutcome result = await searchIndexRepository.UpsertRecipesAsync(
                new()
                {
                    Recipes =
                    [
                        new RecipeSearchIndexInformation()
                        {
                            RecipeId = recipe.RecipeFk,
                            RecipeBookId = recipe.RecipeBookFk,
                            Name = recipe.Name,
                            ShortDescription = recipe.ShortDescription,
                        },
                    ],
                },
                ct
            );

            switch (result)
            {
                case IndexMutationOperationOutcome.Success:
                    await recipeSearchExportStatusRepository.MarkRecipeAsExtractedAndReleaseAsync(
                        recipe.RecipeFk,
                        recipe.SearchVersion,
                        leaseToken,
                        ct
                    );
                    break;
                case IndexMutationOperationOutcome.Error:
                    return;
                case IndexMutationOperationOutcome.BatchFailed:
                    await recipeSearchExportStatusRepository.MarkRecipeExtractionFailedAndReleaseAsync(
                        recipe.RecipeFk,
                        recipe.SearchVersion,
                        leaseToken,
                        ct
                    );
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
