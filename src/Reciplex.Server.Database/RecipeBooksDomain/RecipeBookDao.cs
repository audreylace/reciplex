using NodaTime;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Describes a recipe book
/// </summary>
public class RecipeBookDao
{
    /// <summary>
    /// Books primary key.
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
    /// Id of the user that owns this book.
    /// </summary>
    public required string OwningUserKey { get; init; }

    /// <summary>
    /// The concurrency tag. MUST be transportable via ETAG header.
    /// </summary>
    public required string ConcurrencyTag { get; init; }

    /// <summary>
    /// Last time in UTC the recipe book details were updated
    /// </summary>
    public required Instant LastModified { get; init; }

    /// <summary>
    /// The time in UTC the recipe book was created
    /// </summary>
    public required Instant Created { get; init; }

    /// <summary>
    /// User can edit book information
    /// </summary>
    public bool MayEditBook { get; set; }

    /// <summary>
    /// User can delete book
    /// </summary>
    public bool MayDeleteBook { get; set; }
}
