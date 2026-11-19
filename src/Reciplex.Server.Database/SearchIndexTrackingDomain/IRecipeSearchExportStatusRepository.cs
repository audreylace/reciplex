namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Repository for managing recipe search export status rows.
/// </summary>
/// <remarks>All methods throw database exceptions on failure. Caller is responsible for error flow control.</remarks>
public interface IRecipeSearchExportStatusRepository
{
    /// <summary>
    /// Breaks leases that have expired for recipes and books
    /// </summary>
    /// <param name="leaseMaxLookBack">How far in the past from now to consider leases still valid.</param>
    /// <param name="leaseMaxLookAhead">How far in the future from now to consider leases still valid.</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of leases broken</returns>
    public Task<int> BreakLeasesAsync(
        TimeSpan leaseMaxLookBack,
        TimeSpan leaseMaxLookAhead,
        CancellationToken ct
    );

    /// <summary>
    /// Renews status rows with ids <paramref name="rowIds"/> with matching <paramref name="leaseToken"/>. Sets the new
    /// expire time to <paramref name="expireAfter"/>.
    /// </summary>
    /// <param name="rowIds">set of rows to renew</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="expireAfter">how far from now before the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RenewLeasesAsync(
        List<long> rowIds,
        string leaseToken,
        TimeSpan expireAfter,
        CancellationToken ct
    );

    /// <summary>
    /// Clears leases from rows with ids <paramref name="rowIds"/> with matching <paramref name="leaseToken"/>.
    /// </summary>
    /// <param name="rowIds">set of rows to clear</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> ClearLeasesAsync(List<long> rowIds, string leaseToken, CancellationToken ct);

    /// <summary>
    /// Claims leases on rows with ids <paramref name="rowIds"/> with a <c>null</c> lease token.
    /// Sets the claimed rows lease token to <paramref name="leaseToken" /> and its expire time
    /// to <paramref name="expireAfter" />
    /// </summary>
    /// <param name="rowIds">set of rows to claim</param>
    /// <param name="leaseToken">the lease token identifying this lease</param>
    /// <param name="expireAfter">how far from now before the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows claimed</returns>
    public Task<int> ClaimLeasesAsync(
        List<long> rowIds,
        string leaseToken,
        TimeSpan expireAfter,
        CancellationToken ct
    );

    /// <summary>
    /// Gets the row ids associated with <paramref name="leaseToken"/>
    /// </summary>
    /// <param name="leaseToken">the lease token to back lookup row ids</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set of row primary ids associated with <paramref name="leaseToken"/></returns>
    public Task<List<long>> GetClaimedRowsIdsAsync(string leaseToken, CancellationToken ct);

    /// <summary>
    /// Extracts a set of rows with lease token <paramref name="leaseToken"/>
    /// </summary>
    /// <param name="rowIds">set of rows to claim</param>
    /// <param name="leaseToken">the lease token identifying the lease</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set or extracted rows</returns>
    public Task<List<IExtractedSearchRow>> ExtractRecordsAsync(
        List<long> rowIds,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Gets up to <paramref name="limit"/> rows ids that need to be re-extracted to the search index
    /// </summary>
    /// <param name="limit">max number of rows to extract</param>
    /// <param name="maxRetries">Rows whose retry are at or beyond this are skipped</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set of ids to extract</returns>
    public Task<List<long>> GetPendingRecordsAsync(int limit, int maxRetries, CancellationToken ct);

    /// <summary>
    /// Marks a row as extracted and then releases the lease
    /// </summary>
    /// <param name="rowId">the row id</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RecordExtractionSuccessAsync(
        long rowId,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Marks a row extraction as failed and then releases the lease
    /// </summary>
    /// <param name="rowId">the row id</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="maxRetries">max number of tries for a given version</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RecordExtractionFailureAsync(
        long rowId,
        string leaseToken,
        int maxRetries,
        CancellationToken ct
    );

    /// <summary>
    /// Gets rows to delete
    /// </summary>
    /// <param name="limit">max number of rows</param>
    /// <param name="maxRetries">rows with max tries at or beyond this are ignored</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set of row ids that could be deleted</returns>
    public Task<List<long>> GetRowsToDeleteAsync(int limit, int maxRetries, CancellationToken ct);

    /// <summary>
    /// Deletes rows with max retry count
    /// </summary>
    /// <param name="maxRetries">rows with a retry count or more then this are dropped</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows deleted</returns>
    public Task<int> PurgeRowsWithMaxDeleteAttemptsAsync(int maxRetries, CancellationToken ct);

    /// <summary>
    /// Deletes a set of rows with pending delete status associated with
    /// <paramref name="leaseToken"/> and <paramref name="rowIds"/>.
    /// </summary>
    /// <param name="rowIds">the set of rows to delete</param>
    /// <param name="leaseToken">lease token for identifying which rows are owned</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows deleted</returns>
    public Task<int> FinishRowsDeletionAsync(
        List<long> rowIds,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Records a row deletion attempt as failed
    /// </summary>
    /// <param name="rowId">the row id</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="maxRetries">max number of deletion tries</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RecordRowDeletionFailedAsync(
        long rowId,
        long maxRetries,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Purges all rows from the database that are marked for deletion
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows dropped</returns>
    public Task<int> PurgeAllRowsMarkedForDeletionAsync(CancellationToken ct);

    /// <summary>
    /// Queues all entries associated with <paramref name="bookId"/> with the delete status
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>if any rows were updated</returns>
    public Task<bool> SetBookAndRecipesAsPendingDeleteAsync(long bookId, CancellationToken ct);

    /// <summary>
    /// Cross syncs recipe deletion status setting recipes search index
    /// entries to pending delete as needed. This is used as a backup to guard
    /// against persistent data corruption.
    /// </summary>
    /// <param name="ct">cancels the operation</param>
    /// <returns>number of entries updated</returns>
    Task<int> CrossSyncRecipesDeletionStatusAsync(CancellationToken ct);

    /// <summary>
    /// Signals that there is work for the search synchronization process
    /// </summary>
    void SignalExtractionWork();

    /// <summary>
    /// Signals that there is work for the search synchronization process
    /// </summary>
    void SignalDeletionWork();

    /// <summary>
    /// Signals that there is work for the search synchronization process
    /// </summary>
    /// <param name="timeout">how long to wait</param>
    /// <param name="ct">cancels the operation</param>
    Task WaitForExtractionWorkAsync(TimeSpan timeout, CancellationToken ct);

    /// <summary>
    /// Signals that there is delete work for the search synchronization process
    /// </summary>
    /// <param name="timeout">how long to wait</param>
    /// <param name="ct">cancels the operation</param>
    Task WaitForDeletionWorkAsync(TimeSpan timeout, CancellationToken ct);
}
