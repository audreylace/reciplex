namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// Repository for managing recipe search export status rows.
/// </summary>
/// <remarks>All methods throw database exceptions on failure. Caller is responsible for error flow control.</remarks>
interface IRecipeSearchExportStatusRepository
{
    /// <summary>
    /// Breaks up to <paramref name="max"/> leases that have expired.
    /// </summary>
    /// <param name="max">number of leases to break</param>
    /// <param name="leaseMaxLookBack">How far in the past from now to consider leases still valid.</param>
    /// <param name="leaseMaxLookAhead">How far in the future from now to consider leases still valid.</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of leases broken</returns>
    public Task<int> BreakLeasesAsync(
        int max,
        long leaseMaxLookBack,
        long leaseMaxLookAhead,
        CancellationToken ct
    );

    /// <summary>
    /// Renews rows with ids <paramref name="ids"/> with matching <paramref name="leaseToken"/>. Sets the new
    /// expire time to <paramref name="leaseExpireTime"/>.
    /// </summary>
    /// <param name="ids">set of rows to renew</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="leaseExpireTime">the new lease expire time</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RenewLeasesAsync(
        List<long> ids,
        string leaseToken,
        long leaseExpireTime,
        CancellationToken ct
    );

    /// <summary>
    /// Clears leases from rows with ids <paramref name="ids"/> with matching <paramref name="leaseToken"/>.
    /// </summary>
    /// <param name="ids">set of rows to clear</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> ClearLeasesAsync(List<long> ids, string leaseToken, CancellationToken ct);

    /// <summary>
    /// Claims leases on rows with ids <paramref name="ids"/> with a <c>null</c> lease token.
    /// Sets the claimed rows lease token to <paramref name="leaseToken" /> and its expire time
    /// to <paramref name="leaseExpireTime" />
    /// </summary>
    /// <param name="ids">set of rows to claim</param>
    /// <param name="leaseToken">the lease token identifying this lease</param>
    /// <param name="leaseExpireTime">the time the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <param name="extractionHazard">set to true to flip the extraction hazard field.
    /// Extraction hazard is a one way flag that marks any rows that could
    /// possibly have data in the search index.
    /// </param>
    /// <returns>number of rows claimed</returns>
    public Task<int> ClaimAsync(
        List<long> ids,
        string leaseToken,
        long leaseExpireTime,
        CancellationToken ct
    );

    public Task<List<long>> GetClaimedRecipes(string leaseToken, CancellationToken ct);

    /// <summary>
    /// Extracts a set of recipes with lease token <paramref name="leaseToken"/>
    /// </summary>
    /// <param name="ids">set of rows to claim</param>
    /// <param name="leaseToken">the lease token identifying the lease</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set or extracted rows</returns>
    public Task<List<RecipeRecordDataExtractedFromDatabase>> ExtractRecipeDataAsync(
        List<long> ids,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Creates search status rows for recipes that do not have them and takes out a lease on them
    /// </summary>
    /// <param name="max">max number of rows to create</param>
    /// <param name="leaseToken">the lease token identifying this lease</param>
    /// <param name="leaseExpireTime">the time the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>ids of the locked rows for export</returns>
    public Task<List<long>> CreateSearchStatusRowsAsync(
        int max,
        string leaseToken,
        long leaseExpireTime,
        CancellationToken ct
    );

    /// <summary>
    /// Gets up to <paramref name="max"/> recipe ids that need to be re-extracted to the search index
    /// </summary>
    /// <param name="max">max number of recipes to extract</param>
    /// <param name="maxRetries">Recipes whose retry are at or beyond this are skipped</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set of ids to extract</returns>
    public Task<List<long>> GetRecipesToExtractAsync(int max, int maxRetries, CancellationToken ct);

    /// <summary>
    /// Marks a recipe as extracted up to <paramref name="searchVersion"/> and then releases the lease
    /// </summary>
    /// <param name="id">the recipe id</param>
    /// <param name="searchVersion">the recipe search version</param>
    /// <param name="leaseToken">the recipe token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> MarkRecipeAsExtractedAndReleaseAsync(
        long id,
        long searchVersion,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Marks a recipe extract as failed and then releases the lease
    /// </summary>
    /// <param name="id">the recipe id</param>
    /// <param name="searchVersion">the recipe search version</param>
    /// <param name="leaseToken">the recipe token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> MarkRecipeExtractionFailedAndReleaseAsync(
        long id,
        long searchVersion,
        string leaseToken,
        CancellationToken ct
    );

    public Task<List<long>> GetRecipesToDeleteAsync(int max, int maxRetries, CancellationToken ct);
    public Task<int> DeleteRecipeSearchEntries(
        List<long> recipeIds,
        string leaseToken,
        CancellationToken ct
    );
    public Task<int> PurgeRecipeSearchEntriesWithTooManyRetries(
        int max,
        int maxRetries,
        CancellationToken ct
    );

    public Task<int> MarkRecipeDeletionFailedAndReleaseAsync(
        long id,
        string leaseToken,
        CancellationToken ct
    );
}
