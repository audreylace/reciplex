namespace Reciplex.Server.Database.SearchExporter.Repositories;

class RecipeSearchDao
{
    /// <summary>
    /// The recipe id of this search index entry.
    /// </summary>
    public required long RecipeId { get; set; }

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
}
