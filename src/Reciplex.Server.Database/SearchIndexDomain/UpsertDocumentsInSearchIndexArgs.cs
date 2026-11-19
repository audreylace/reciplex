namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Args for
/// <see cref="ISearchIndexRepository.UpsertDocumentsAsync(UpsertDocumentsInSearchIndexArgs, CancellationToken)"/>
/// </summary>
public sealed class UpsertDocumentsInSearchIndexArgs
{
    /// <summary>
    /// Set of recipe documents to upsert
    /// </summary>
    public ICollection<IRecipeOrBookSearchDao> Documents { get; set; } = [];
}
