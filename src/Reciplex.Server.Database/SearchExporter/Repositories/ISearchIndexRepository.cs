namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// Repository for a search index
/// </summary>
interface ISearchIndexRepository
{
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
    /// Runs any one time search index setup for the recipe index
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the index exists or was created. False if the index creation failed.</returns>
    public Task<bool> EnsureRecipeIndexSetupCompleteAsync(CancellationToken ct);

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
}
