namespace Reciplex.Server.Database.RecipeBooksDomain;

public class RecipeBookDeletionQueueEntry
{
    /// <summary>
    /// Books primary key.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// The concurrency tag. MUST be transportable via ETAG header.
    /// </summary>
    public required string ConcurrencyTag { get; init; }
}
