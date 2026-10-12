namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Args for
/// <see cref="ISearchIndexRepository.UpsertRecipesAsync(Reciplex.Server.Database.SearchIndexDomain.UpsertRecipesInSearchIndexArgs, CancellationToken)"/>
/// </summary>
public sealed class UpsertRecipesInSearchIndexArgs
{
    /// <summary>
    /// Set of recipe documents to upsert
    /// </summary>
    public ICollection<RecipeSearchDao> Recipes { get; set; } = [];
}
