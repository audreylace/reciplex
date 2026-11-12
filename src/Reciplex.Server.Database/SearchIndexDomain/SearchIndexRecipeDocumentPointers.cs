namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Pointer to a recipe search index document
/// </summary>
public sealed class SearchIndexRecipeDocumentPointers : SearchIndexDocumentPointer
{
    /// <summary>
    /// The id of the book holding the recipe
    /// </summary>
    public required long BookId { get; init; }

    /// <summary>
    /// The id of the recipe
    /// </summary>
    public required long RecipeId { get; init; }

    /// <inheritdoc />
    public override RecipeBookSearchIndexDocumentKind Kind =>
        RecipeBookSearchIndexDocumentKind.Recipe;
}
