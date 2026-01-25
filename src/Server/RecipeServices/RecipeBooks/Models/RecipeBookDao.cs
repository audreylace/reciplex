namespace Reciplex.Server.RecipeServices.RecipeBooks.Models;

/// <summary>
/// Describes a recipe book
/// </summary>
public class RecipeBookDao
{
    /// <summary>
    /// Books primary key. Must be URL transportable.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the book
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Book short description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// Id of the user that owns this book. Must be URL transportable.
    /// </summary>
    public required string OwnerUserId { get; init; }

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
    /// Book access permissions
    /// </summary>
    public required RecipeBookAccessPermissions AccessPermissions { get; init; }
}
