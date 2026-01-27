namespace Reciplex.Server.RecipeServices.Recipes.Models;

/// <summary>
/// Information about a recipe
/// </summary>
public class RecipeDao
{
    /// <summary>
    /// Recipe primary key. Must be URL transportable.
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
    /// The concurrency tag. MUST be transportable via ETAG header.
    /// </summary>
    public required string ConcurrencyTag { get; init; }

    /// <summary>
    /// Last time in UTC the recipe book details were updated
    /// </summary>
    public required DateTime LastUpdated { get; init; }

    /// <summary>
    /// The time in UTC the recipe book was created
    /// </summary>
    public required DateTime CreateTime { get; init; }

    /// <summary>
    /// Recipe access permissions for this user. May or may not align with the permission
    /// in <see cref="RecipeBookDao"/>.
    /// </summary>
    public required RecipeAccessPermissions AccessPermissions { get; init; }

    /// <summary>
    /// The book this recipe is contained in
    /// </summary>
    public required string BookId { get; init; }

    /// <summary>
    /// The long, freeform, markdown recipe data
    /// </summary>
    public required string Details { get; set; }
}
