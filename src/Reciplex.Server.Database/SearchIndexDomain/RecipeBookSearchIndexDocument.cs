using System.Text.Json.Serialization;

namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Search index document for recipe books and their recipes stored in the backend search
/// </summary>
public sealed class RecipeBookSearchIndexDocument
{
    /// <summary>
    /// The id of the entry.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// The recipe id of this search index entry. Only set when <see cref="Kind" /> is <see cref="RecipeBookSearchIndexDocumentKind.Recipe" />.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? RecipeId { get; set; }

    /// <summary>
    /// The kind of the document
    /// </summary>
    public required RecipeBookSearchIndexDocumentKind Kind { get; set; }

    /// <summary>
    /// The name of the recipe or book
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The recipe or book's short description
    /// </summary>
    public required string ShortDescription { get; set; }

    /// <summary>
    /// The main parent book id. Always set for both kinds.
    /// </summary>
    public required long RecipeBookId { get; set; }

    /// <summary>
    /// Builds a book document for the search index
    /// </summary>
    /// <param name="rowId">the source row tracking search export status</param>
    /// <param name="bookId">id of the book to store</param>
    /// <param name="name">name of the book</param>
    /// <param name="description">the book's description</param>
    /// <returns>the book document for storage in the search index</returns>
    public static RecipeBookSearchIndexDocument ForBook(
        long rowId,
        long bookId,
        string name,
        string description
    ) =>
        new()
        {
            Id = rowId,
            Kind = RecipeBookSearchIndexDocumentKind.RecipeBook,
            RecipeBookId = bookId,
            RecipeId = null,
            Name = name,
            ShortDescription = description,
        };

    /// <summary>
    /// Builds a recipe document for the search index
    /// </summary>
    /// <param name="rowId">the source row tracking search export status</param>
    /// <param name="recipeId">id of the recipe to store</param>
    /// <param name="bookId">id of the book the recipe belongs to</param>
    /// <param name="name">recipe name</param>
    /// <param name="description">recipe description</param>
    /// <returns>the recipe document for storage in the search index</returns>
    public static RecipeBookSearchIndexDocument ForRecipe(
        long rowId,
        long recipeId,
        long bookId,
        string name,
        string description
    ) =>
        new()
        {
            Id = rowId,
            Kind = RecipeBookSearchIndexDocumentKind.Recipe,
            RecipeBookId = bookId,
            RecipeId = recipeId,
            Name = name,
            ShortDescription = description,
        };
}
