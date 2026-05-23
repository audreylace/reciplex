namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Information about a recipe inside a list result
/// </summary>
public class RecipeListEntryDao
{
    /// <summary>
    /// Recipe primary key.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the recipe
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Recipe short description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// The book this recipe is contained in
    /// </summary>
    public required string BookId { get; init; }
}
