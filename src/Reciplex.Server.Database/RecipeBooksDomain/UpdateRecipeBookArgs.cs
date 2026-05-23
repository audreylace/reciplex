namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Arguments for updating a recipe book
/// </summary>
public class UpdateRecipeBookArgs
{
    /// <summary>
    /// New name of the book
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// New short description of the book
    /// </summary>
    public required string ShortDescription { get; set; }

    /// <summary>
    /// The expected concurrency tag. Operation will
    /// fail if the one stored on the database does not match.
    /// </summary>
    public required string ConcurrencyTag { get; set; }
}
