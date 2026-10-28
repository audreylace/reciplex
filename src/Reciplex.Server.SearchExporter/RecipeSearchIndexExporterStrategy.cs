using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Strategy for exporting a set of rows to the search index
/// </summary>
/// <param name="recipeSearchExportStatusRepository">search status repository</param>
/// <param name="searchIndexRepository">repository representing the search index</param>
/// <param name="leaseRenewer">strategy for renewing leases held while extraction takes place</param>
/// <param name="logger">instance logger</param>
sealed class RecipeSearchIndexExporterStrategy(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository,
    LeaseRenewer leaseRenewer,
    ILogger<RecipeSearchIndexExporterStrategy> logger
)
{
    /// <summary>
    /// Exports a set of rows keyed by <paramref name="rowIds"/> with lease <paramref name="leaseToken"/>.
    /// On completion, database is updated and lease cleared.
    /// </summary>
    /// <param name="rowIds">set of row primary ids</param>
    /// <param name="leaseToken">the token of the held lease</param>
    /// <param name="maxExportAttempts">max number of export attempts</param>
    /// <param name="ct">async cancellation token</param>
    public async Task ExportRowsAsync(
        List<long> rowIds,
        string leaseToken,
        int maxExportAttempts,
        CancellationToken ct
    )
    {
        List<IExtractedSearchRow> dataToExport =
            await recipeSearchExportStatusRepository.ExtractRecordsAsync(rowIds, leaseToken, ct);
        if (dataToExport.Count < 1)
        {
            return; // someone broke our leases or they expired
        }

        List<long> idsToUnlock = [.. dataToExport.Select(d => d.RowId)];
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        var renewerTask = RenewIdsPeriodicallyAsync(
            leaseToken,
            idsToUnlock,
            cancellationTokenSource
        );

        List<long> idsExtracted = [];
        List<long> idsFailed = [];
        try
        {
            switch (
                await searchIndexRepository.UpsertDocumentsAsync(
                    new()
                    {
                        Documents =
                        [
                            .. dataToExport
                                .Where(row =>
                                    row is RecipeRecordDataExtractedFromDatabase
                                    || row is BookRecordDataExtractedFromDatabase
                                )
                                .Select(MapIntoSearchDao),
                        ],
                    },
                    cancellationTokenSource.Token
                )
            )
            {
                case IndexMutationOperationOutcome.Success:
                    idsExtracted = [.. idsToUnlock];
                    idsToUnlock = [];
                    break;
                case IndexMutationOperationOutcome.BatchFailed:
                    (idsToUnlock, idsExtracted, idsFailed) = await ExtractInSerial(
                        dataToExport,
                        cancellationTokenSource.Token
                    );
                    break;
            }
        }
        catch (Exception ex)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await cancellationTokenSource.CancelAsync();
                }
                catch (Exception innerEx)
                {
                    logger.Error_CancelAsyncFailed(innerEx);
                }
            }

            try
            {
                await renewerTask;
            }
            catch (OperationCanceledException) { }
            catch (Exception innerEx)
            {
                logger.Error_RenewTaskFailed(innerEx);
            }

            try
            {
                if (ct.IsCancellationRequested) // release locks on shutdown
                {
                    using CancellationTokenSource fiveSecondCancellationSource = new();
                    fiveSecondCancellationSource.CancelAfter(TimeSpan.FromSeconds(5));
                    await UpdateSearchRowsAsync(
                        leaseToken,
                        rowIds,
                        [],
                        [],
                        maxExportAttempts,
                        fiveSecondCancellationSource.Token
                    );
                }
                else
                {
                    await UpdateSearchRowsAsync(leaseToken, rowIds, [], [], maxExportAttempts, ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception innerEx)
            {
                logger.Error_LeaseReleaseFailed(innerEx);
            }

            if (ex is not OperationCanceledException && ct.IsCancellationRequested)
            {
                return;
            }

            throw;
        }

        await UpdateSearchRowsAsync(
            leaseToken,
            idsToUnlock,
            idsExtracted,
            idsFailed,
            maxExportAttempts,
            ct
        );
    }

    /// <summary>
    /// Maps a <see cref="IExtractedSearchRow"/> into a <see cref="IRecipeOrBookSearchDao"/>
    /// </summary>
    /// <param name="searchData">the instance to map</param>
    /// <returns>the mapped data</returns>
    /// <exception cref="NotImplementedException">the exception thrown if the derived type <see cref="IExtractedSearchRow"/> is not recognized</exception>
    private static IRecipeOrBookSearchDao MapIntoSearchDao(IExtractedSearchRow searchData)
    {
        return searchData switch
        {
            RecipeRecordDataExtractedFromDatabase recipe => new RecipeSearchDao()
            {
                RowId = recipe.RowId,
                RecipeId = recipe.RecipeFk,
                RecipeBookId = recipe.RecipeBookFk,
                Name = recipe.Name,
                ShortDescription = recipe.ShortDescription,
            },
            BookRecordDataExtractedFromDatabase book => new RecipeBookSearchDao()
            {
                RowId = book.RowId,
                RecipeBookId = book.RecipeBookFk,
                Name = book.Name,
                ShortDescription = book.ShortDescription,
            },
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Extracts records in serial
    /// </summary>
    /// <param name="extractedRowData">the extracted row data</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>Tuple where the first is the set of ids to unlock. These were not attempted and should be retried without incrementing their failure count again.
    /// Second tuple value is the ids that were extracted.
    /// Third and final tuple value is the ids that failed to extract.
    /// </returns>
    private async Task<(
        List<long> IdsToUnlock,
        List<long> IdsExtracted,
        List<long> IdsFailed
    )> ExtractInSerial(List<IExtractedSearchRow> extractedRowData, CancellationToken ct)
    {
        HashSet<long> idsToUnlock = [.. extractedRowData.Select(d => d.RowId)];
        List<long> idsExtracted = [];
        List<long> idsFailed = [];

        foreach (IExtractedSearchRow row in extractedRowData)
        {
            if (ct.IsCancellationRequested)
            {
                return new([.. idsToUnlock], idsExtracted, idsFailed);
            }

            if (
                row is not RecipeRecordDataExtractedFromDatabase
                && row is not BookRecordDataExtractedFromDatabase
            )
            {
                idsExtracted.Add(row.RowId);
                idsToUnlock.Remove(row.RowId);
                continue;
            }

            try
            {
                switch (
                    await searchIndexRepository.UpsertDocumentsAsync(
                        new() { Documents = [MapIntoSearchDao(row)] },
                        ct
                    )
                )
                {
                    case IndexMutationOperationOutcome.Success:
                        idsExtracted.Add(row.RowId);
                        idsToUnlock.Remove(row.RowId);
                        break;

                    case IndexMutationOperationOutcome.BatchFailed:
                        idsFailed.Add(row.RowId);
                        idsToUnlock.Remove(row.RowId);
                        logger.Error_SearchExportFailed(row.RowId);
                        break;

                    default:
                        return new([.. idsToUnlock], idsExtracted, idsFailed);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Error_SearchExportFailed(row.RowId, ex);
                idsFailed.Add(row.RowId);
                idsToUnlock.Remove(row.RowId);
                await SafeDelay.DelayAsync(TimeSpan.FromMilliseconds(100), ct);
            }
        }

        return new([.. idsToUnlock], idsExtracted, idsFailed);
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
            catch (Exception ex)
            {
                logger.Error_CancelAsyncFailed(ex);
            }
        }
    }

    /// <summary>
    /// Releases leases and records export outcomes to database.
    /// <paramref name="idsThatFailed"/>, <paramref name="idsExtracted"/>, <paramref name="idsToUnlock"/>
    /// should not have any overlapping ids.
    /// </summary>
    /// <param name="leaseToken">token identifying the lease</param>
    /// <param name="idsToUnlock">rows whose lease should be cleared and no outcome recorded</param>
    /// <param name="idsExtracted">rows dropped from the search index and whose rows should be dropped from the database</param>
    /// <param name="idsThatFailed">rows whose deletion failed</param>
    /// <param name="maxAttempts">max number of export attempts</param>
    /// <param name="ct">cancels the async operation</param>
    private async Task UpdateSearchRowsAsync(
        string leaseToken,
        List<long> idsToUnlock,
        List<long> idsExtracted,
        List<long> idsThatFailed,
        int maxAttempts,
        CancellationToken ct
    )
    {
        if (idsToUnlock.Count > 0)
        {
            try
            {
                await recipeSearchExportStatusRepository.ClearLeasesAsync(
                    idsToUnlock,
                    leaseToken,
                    ct
                );
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_LeaseReleaseFailed(ex);
            }
        }

        if (idsExtracted.Count > 0)
        {
            foreach (long rowId in idsExtracted)
            {
                try
                {
                    await recipeSearchExportStatusRepository.RecordExtractionSuccessAsync(
                        rowId,
                        leaseToken,
                        ct
                    );
                }
                catch (Exception ex)
                    when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    logger.Error_MarkingAsExtractedFailed(rowId, ex);
                }
            }
        }

        if (idsThatFailed.Count > 0)
        {
            foreach (long rowId in idsThatFailed)
            {
                try
                {
                    await recipeSearchExportStatusRepository.RecordExtractionFailureAsync(
                        rowId,
                        leaseToken,
                        maxAttempts,
                        ct
                    );
                }
                catch (Exception ex)
                    when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    logger.Error_IncrementingExtractionAttemptFailed(rowId, ex);
                }
            }
        }
    }
}
