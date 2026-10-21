using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;

namespace Reciplex.Server.SearchExporter.SearchBackgroundTasks;

/// <summary>
/// Purges rows from the search index periodically
/// </summary>
/// <param name="exportStatusRepo">tracks status of data exportation to the search index</param>
/// <param name="searchIndexRepo">repository of search data</param>
/// <param name="options">search index application options</param>
/// <param name="concurrencyTagProvider">Used to generate lease tokens</param>
/// <param name="leaseRenewer">instance for renewing leases</param>
/// <param name="logger">instance logger</param>
sealed class RowsDeletionSearchBackgroundTask(
    IRecipeSearchExportStatusRepository exportStatusRepo,
    ISearchIndexRepository searchIndexRepo,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider concurrencyTagProvider,
    LeaseRenewer leaseRenewer,
    ILogger<RowsDeletionSearchBackgroundTask> logger
) : ISearchBackgroundTask
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // look for potential rows that could be deleted from the search index
                List<long> entries = await exportStatusRepo.GetRowsToDeleteAsync(
                    options.Value.SearchDeleteBatchSize,
                    options.Value.MaxRecipeDeleteAttempts,
                    stoppingToken
                );

                // delay if no rows were found, otherwise delete them
                if (entries.Count < 1)
                {
                    await exportStatusRepo.WaitForDeletionWorkAsync(
                        TimeSpan.FromMinutes(1),
                        stoppingToken
                    );
                }
                else
                {
                    await DeleteRowsFromIndexAsync(entries, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit on shutdown
            }
            catch (Exception ex)
            {
                logger.Error_DeleteRowsFromIndexFailed(ex);

                // delay to prevent a tight loop on repeated failure
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Attempts to claim some subset of <paramref name="candidates"/> and then delete them from the search index
    /// </summary>
    /// <param name="candidates">ids to claim and delete</param>
    /// <param name="ct">cancels the operation</param>
    private async Task DeleteRowsFromIndexAsync(List<long> candidates, CancellationToken ct)
    {
        // generate a unique lease token to identify our lease
        string leaseToken = concurrencyTagProvider.NextTag();

        if (
            await exportStatusRepo.ClaimLeasesAsync(
                candidates,
                leaseToken,
                TimeSpan.FromMinutes(5),
                ct
            ) < 1
        )
        {
            return; // claimed nothing; exit
        }

        // determine the actual ids of claimed rows as its not guaranteed that we got all of candidates
        List<long> claimedIds = await exportStatusRepo.GetClaimedRowsIdsAsync(leaseToken, ct);
        if (claimedIds.Count < 1)
        {
            return;
        }

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);

        // renew held rows in a background task
        Task renewerTask = RenewIdsPeriodicallyAsync(
            leaseToken,
            claimedIds,
            cancellationTokenSource
        );

        bool delay = false;

        // Ensure unlock on exception to minimize delays.
        // Otherwise rows will be locked up to 10 minutes
        // resulting in end user delays.
        List<long> rowsToUnlock = claimedIds;
        List<long> rowsToDelete = [];
        List<long> rowsThatFailed = [];
        try
        {
            switch (
                await searchIndexRepo.DeleteDocumentsAsync(
                    claimedIds,
                    cancellationTokenSource.Token
                )
            )
            {
                case IndexMutationOperationOutcome.Success:
                    rowsToUnlock = [];
                    rowsToDelete = claimedIds; // delete will unlock for us
                    break;

                case IndexMutationOperationOutcome.Error:
                    delay = true; // pause to avoid tight loop during search index outage
                    break;

                case IndexMutationOperationOutcome.BatchFailed:
                    // Try one by one in case the failure was caused by one or several in the batch.
                    // The search index aborts the batch upload when one or more failed.
                    (rowsToDelete, rowsThatFailed, rowsToUnlock) =
                        await DeleteRowsFromIndexInSerialAsync(
                            claimedIds,
                            cancellationTokenSource.Token
                        );
                    break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.Error_DeleteRowsFromIndexFailed(ex);
            delay = true;
        }

        try
        {
            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
            catch (ObjectDisposedException) { }

            await renewerTask;
        }
        catch (Exception ex)
        {
            logger.Error_RenewTaskFailed(ex);
        }

        if (ct.IsCancellationRequested || delay) // release locks on shutdown or error
        {
            using CancellationTokenSource cancellation = new();
            cancellation.CancelAfter(TimeSpan.FromSeconds(5));
            await UpdateSearchRowsAsync(leaseToken, claimedIds, [], [], cancellation.Token);
        }
        else
        {
            // records outcomes to the database and clear leases
            await UpdateSearchRowsAsync(leaseToken, rowsToUnlock, rowsToDelete, rowsThatFailed, ct);
        }

        if (delay && !ct.IsCancellationRequested)
        {
            await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), ct);
        }
    }

    /// <summary>
    /// Renews leases on rows periodically until the token bound to <paramref name="cancellationTokenSource"/>
    /// is cancelled.
    /// </summary>
    /// <param name="leaseToken">token identifying our lease</param>
    /// <param name="claimedIds">the ids to renew</param>
    /// <param name="cancellationTokenSource">token cancellation source. Cancelled when the renewer task returns.</param>
    private async Task RenewIdsPeriodicallyAsync(
        string leaseToken,
        List<long> claimedIds,
        CancellationTokenSource cancellationTokenSource
    )
    {
        try
        {
            await leaseRenewer.ExecuteAsync(
                leaseToken,
                claimedIds,
                TimeSpan.FromMinutes(5), // lease is valid for 5 minutes
                TimeSpan.FromSeconds(30), // renew every 30 seconds
                8, // try renew up to 8 times on failure
                cancellationTokenSource.Token
            );
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        { }
        finally
        {
            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
            catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// Releases leases and records export outcomes to database.
    /// <paramref name="rowsThatFailed"/>, <paramref name="rowsToDelete"/>, <paramref name="rowsToUnlock"/>
    /// should not have any overlapping ids.
    /// </summary>
    /// <param name="leaseToken">token identifying the lease</param>
    /// <param name="rowsToUnlock">rows whose lease should be cleared and no outcome recorded</param>
    /// <param name="rowsToDelete">rows dropped from the search index and whose rows should be dropped from the database</param>
    /// <param name="rowsThatFailed">rows whose deletion failed</param>
    /// <param name="ct">cancels the async operation</param>
    private async Task UpdateSearchRowsAsync(
        string leaseToken,
        List<long> rowsToUnlock,
        List<long> rowsToDelete,
        List<long> rowsThatFailed,
        CancellationToken ct
    )
    {
        if (rowsToUnlock.Count > 0)
        {
            try
            {
                await exportStatusRepo.ClearLeasesAsync(rowsToUnlock, leaseToken, ct);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_FailedToClearLeases(ex);
            }
        }

        if (rowsToDelete.Count > 0)
        {
            try
            {
                await exportStatusRepo.FinishRowsDeletionAsync(rowsToDelete, leaseToken, ct);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_FailedToDeleteRows(ex);
            }
        }

        if (rowsThatFailed.Count > 0)
        {
            foreach (long failedRowId in rowsThatFailed)
            {
                try
                {
                    await exportStatusRepo.RecordRowDeletionFailedAsync(
                        failedRowId,
                        options.Value.MaxRecipeDeleteAttempts,
                        leaseToken,
                        ct
                    );
                }
                catch (Exception ex)
                    when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    logger.Error_FailedToIncrementRowsFailureCounter(failedRowId, ex);
                }
            }
        }
    }

    /// <summary>
    /// Deletes rows from the search index one by one recording the outcome of each
    /// </summary>
    /// <param name="rowIds">set of ids</param>
    /// <param name="ct">cancels the async operation</param>
    /// <returns>
    /// tuple: first is the rows dropped from the index, second set of rows whose drop outcome failed,
    /// third is the set of rows skipped because <paramref name="ct"/> was cancelled or
    /// because of a network error.
    /// </returns>
    private async Task<(
        List<long> RowsToDelete,
        List<long> RowsThatFailed,
        List<long> RowsToUnlock
    )> DeleteRowsFromIndexInSerialAsync(List<long> rowIds, CancellationToken ct)
    {
        List<long> rowsToDelete = [];
        List<long> rowsThatFailed = [];
        HashSet<long> rowsHandled = [.. rowIds];

        foreach (long rowId in rowIds)
        {
            if (ct.IsCancellationRequested)
            {
                return new(rowsToDelete, rowsThatFailed, [.. rowsHandled]);
            }

            try
            {
                switch (await searchIndexRepo.DeleteDocumentsAsync([rowId], ct))
                {
                    case IndexMutationOperationOutcome.BatchFailed:
                        rowsThatFailed.Add(rowId);
                        rowsHandled.Remove(rowId);
                        logger.Error_DeletingRowFromIndexFailed(rowId);
                        break;
                    case IndexMutationOperationOutcome.Success:
                        rowsToDelete.Add(rowId);
                        rowsHandled.Remove(rowId);
                        break;
                    default:
                        return new(rowsToDelete, rowsThatFailed, [.. rowsHandled]);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                rowsThatFailed.Add(rowId);
                rowsHandled.Remove(rowId);
                logger.Error_DeletingRowFromIndexFailed(rowId, ex);
                await SafeDelay.DelayAsync(TimeSpan.FromMilliseconds(100), ct);

                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        return new(rowsToDelete, rowsThatFailed, [.. rowsHandled]);
    }
}
