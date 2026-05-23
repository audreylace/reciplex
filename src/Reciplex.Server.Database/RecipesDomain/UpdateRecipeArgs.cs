namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Args for updating a recipe
/// </summary>
public class UpdateRecipeArgs
{
    /// <summary>
    /// new name for recipe
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// new short description for recipe
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// new details for the recipe
    /// </summary>
    public required string Details { get; init; }
}
