namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Pointer to a recipe book
/// </summary>
public sealed class SearchIndexBookDocumentPointers : SearchIndexDocumentPointer
{
    /// <summary>
    /// The id of the book
    /// </summary>
    public required long BookId { get; init; }

    /// <inheritdoc />
    public override RecipeBookSearchIndexDocumentKind Kind =>
        RecipeBookSearchIndexDocumentKind.RecipeBook;
}
