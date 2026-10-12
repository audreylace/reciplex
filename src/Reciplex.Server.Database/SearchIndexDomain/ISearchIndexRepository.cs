namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Repository for a search index
/// </summary>
public interface ISearchIndexRepository
{
    /// <summary>
    /// Ensures one time setup is complete
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true on success. false on failure. Search index should be considered offline.</returns>
    public Task<bool> EnsureSetupCompleteAsync(CancellationToken ct);

    /// <summary>
    /// Upserts a set of recipes into the search index as one atomic transaction
    /// </summary>
    /// <param name="args">the upsert arguments</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the batch succeed. False otherwise.</returns>
    public Task<IndexMutationOperationOutcome> UpsertRecipesAsync(
        UpsertRecipesInSearchIndexArgs args,
        CancellationToken ct
    );

    /// <summary>
    /// Deletes a set of recipes from the index
    /// </summary>
    /// <param name="recipeIds">the set of ids</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the batch succeed. False otherwise.</returns>
    public Task<IndexMutationOperationOutcome> DeleteRecipesAsync(
        IEnumerable<long> recipeIds,
        CancellationToken ct
    );

    /// <summary>
    /// If the search feature is enabled
    /// </summary>
    /// <returns>true if enabled</returns>
    public bool IsEnabled();

    /// <summary>
    /// Runs a recipe search
    /// </summary>
    /// <param name="args">args for the search</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>list of search results</returns>
    public Task<List<RecipeBookSearchIndexDocument>> SearchRecipesAsync(
        SearchRecipesIndexArgs args,
        CancellationToken ct
    );
}
