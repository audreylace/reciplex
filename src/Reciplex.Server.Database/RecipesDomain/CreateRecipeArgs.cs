namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Args for creating a recipe
/// </summary>
public class CreateRecipeArgs
{
    /// <summary>
    /// Name of the recipe
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// recipe short description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// Recipe details
    /// </summary>
    public required string Details { get; init; }
}
