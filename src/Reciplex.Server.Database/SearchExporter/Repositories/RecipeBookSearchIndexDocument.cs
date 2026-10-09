using System.Text.Json.Serialization;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// Search index document for recipe books and their recipes stored in the backend search
/// </summary>
sealed class RecipeBookSearchIndexDocument
{
    /// <summary>
    /// The id of the entry.
    /// </summary>
    public required string Id { get; set; }

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

    public static string AsRecipeId(long recipeId) => $"r_{recipeId}";

    public static string AsBookId(long bookId) => $"b_{bookId}";

    public static RecipeBookSearchIndexDocument ForBook(
        long bookId,
        string name,
        string description
    ) =>
        new()
        {
            Id = AsBookId(bookId),
            Kind = RecipeBookSearchIndexDocumentKind.RecipeBook,
            RecipeBookId = bookId,
            RecipeId = null,
            Name = name,
            ShortDescription = description,
        };

    public static RecipeBookSearchIndexDocument ForRecipe(
        long recipeId,
        long bookId,
        string name,
        string description
    ) =>
        new()
        {
            Id = AsRecipeId(recipeId),
            Kind = RecipeBookSearchIndexDocumentKind.Recipe,
            RecipeBookId = bookId,
            RecipeId = recipeId,
            Name = name,
            ShortDescription = description,
        };
}
