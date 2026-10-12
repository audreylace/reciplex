using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexTrackingDomain;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Hosted service that finishes deletion of recipe books
/// </summary>
/// <param name="logger">logger for this instance</param>
/// <param name="booksService">service for manipulating database objects</param>
sealed class RecipeBookDeletionWorkerHostedService(
    ILogger<RecipeBookDeletionWorkerHostedService> logger,
    IRecipeBooksService booksService,
    IRecipesService recipesService,
    IRecipeSearchExportStatusRepository searchExportStatusRepository
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Info_RecipeBookLoopStarting();
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunLoopOnceAsync(stoppingToken);
        }
        logger.Info_RecipeBookLoopStopping();
    }

    /// <summary>
    /// Runs the loop once
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    private async Task RunLoopOnceAsync(CancellationToken ct)
    {
        try
        {
            // get next book off the delete queue
            DatabaseResultVariant<
                EmptySuccessResult,
                SuccessResult<RecipeBookDeletionQueueEntry>
            > queueEntry = await booksService.GetNextBookQueuedForDeletionAsync(ct);

            if (queueEntry.Result is SuccessResult<RecipeBookDeletionQueueEntry> successResult)
            {
                await TryDeleteBookAsync(successResult.Value, ct);
            }
            else
            {
                await booksService.WaitForDeleteSignal(TimeSpan.FromMinutes(1), ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            // delay to prevent a tight loop on repeated failure
            logger.Error_BackgroundLoopFailure(ex);
            await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), ct);
        }
    }

    /// <summary>
    /// Attempts final purge of recipe book
    /// </summary>
    /// <param name="queueEntry">the book</param>
    /// <param name="ct">async cancellation token</param>
    private async Task TryDeleteBookAsync(
        RecipeBookDeletionQueueEntry queueEntry,
        CancellationToken ct
    )
    {
        try
        {
            bool waitOnRecipeDeleteQueue =
                await recipesService.QueueRecipesInBookForDeletion(queueEntry.Id, ct) > 0;

            bool waitOnSearchExtractionStatus =
                await searchExportStatusRepository.MarkExtractionStatusForBookAsPendingDeleteAsync(
                    queueEntry.Id,
                    ct
                ) > 0;

            await recipesService.PurgeDeletedUnreferencedRecipesAsync(queueEntry.Id, ct);

            bool hasPendingRecipes = await recipesService.AnyRecipesInBookAsync(queueEntry.Id, ct);

            if (waitOnRecipeDeleteQueue || waitOnSearchExtractionStatus || hasPendingRecipes)
            {
                await booksService.RequeueRecipesBooksForDeletionAsync(
                    queueEntry.Id,
                    queueEntry.ConcurrencyTag,
                    ct
                );
                return;
            }

            DatabaseResultVariant<EmptySuccessResult, ConflictResult> result =
                await booksService.PurgeRecipeBookAsync(
                    queueEntry.Id,
                    queueEntry.ConcurrencyTag,
                    ct
                );

            if (result.Result is ConflictResult)
            {
                await booksService.RequeueRecipesBooksForDeletionAsync(
                    queueEntry.Id,
                    queueEntry.ConcurrencyTag,
                    ct
                );
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            logger.Error_BookDeleteFailed(queueEntry.Id, ex);
            await booksService.RequeueRecipesBooksForDeletionAsync(
                queueEntry.Id,
                queueEntry.ConcurrencyTag,
                ct
            );
        }
    }
}
