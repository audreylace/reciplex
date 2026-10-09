namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// Repository for managing recipe search export status rows.
/// </summary>
/// <remarks>All methods throw database exceptions on failure. Caller is responsible for error flow control.</remarks>
interface IRecipeSearchExportStatusRepository
{
    /// <summary>
    /// Breaks up to <paramref name="batchSize"/> leases that have expired.
    /// </summary>
    /// <param name="batchSize">number of leases to break</param>
    /// <param name="leaseMaxLookBack">How far in the past from now to consider leases still valid.</param>
    /// <param name="leaseMaxLookAhead">How far in the future from now to consider leases still valid.</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of leases broken</returns>
    public Task<int> BreakLeasesAsync(
        int batchSize,
        TimeSpan leaseMaxLookBack,
        TimeSpan leaseMaxLookAhead,
        CancellationToken ct
    );

    /// <summary>
    /// Renews rows with ids <paramref name="ids"/> with matching <paramref name="leaseToken"/>. Sets the new
    /// expire time to <paramref name="expireAfter"/>.
    /// </summary>
    /// <param name="ids">set of rows to renew</param>
    /// <param name="leaseToken">the lease token</param>
    /// <param name="expireAfter">how far from now before the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> RenewLeasesAsync(
        List<long> ids,
        string leaseToken,
        TimeSpan expireAfter,
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
    /// to <paramref name="expireAfter" />
    /// </summary>
    /// <param name="ids">set of rows to claim</param>
    /// <param name="leaseToken">the lease token identifying this lease</param>
    /// <param name="expireAfter">how far from now before the lease expires</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows claimed</returns>
    public Task<int> ClaimAsync(
        List<long> ids,
        string leaseToken,
        TimeSpan expireAfter,
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
    /// Gets up to <paramref name="batchSize"/> recipe ids that need to be re-extracted to the search index
    /// </summary>
    /// <param name="batchSize">max number of recipes to extract</param>
    /// <param name="maxRetries">Recipes whose retry are at or beyond this are skipped</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>set of ids to extract</returns>
    public Task<List<long>> GetRecipesToExtractAsync(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    );

    /// <summary>
    /// Marks a recipe as extracted and then releases the lease
    /// </summary>
    /// <param name="id">the recipe id</param>
    /// <param name="leaseToken">the recipe token</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> MarkRecipeAsExtractedAndReleaseAsync(
        long id,
        string leaseToken,
        CancellationToken ct
    );

    /// <summary>
    /// Marks a recipe extract as failed and then releases the lease
    /// </summary>
    /// <param name="id">the recipe id</param>
    /// <param name="leaseToken">the recipe token</param>
    /// <param name="maxRetries">max number of tries for a given version before giving up</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows updated</returns>
    public Task<int> MarkRecipeExtractionFailedAndReleaseAsync(
        long id,
        string leaseToken,
        int maxRetries,
        CancellationToken ct
    );

    public Task<List<long>> GetRecipesToDeleteAsync(int max, int maxRetries, CancellationToken ct);
    public Task<int> DeleteRecipeSearchEntries(
        List<long> recipeIds,
        string leaseToken,
        CancellationToken ct
    );
    public Task<int> PurgeRecipeSearchEntriesWithTooManyDeleteRetries(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    );

    public Task<int> MarkRecipesDeletionFailedAndReleaseAsync(
        List<long> recipeIds,
        string leaseToken,
        CancellationToken ct
    );
}
