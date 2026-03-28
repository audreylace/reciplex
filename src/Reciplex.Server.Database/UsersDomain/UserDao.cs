namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Data access object for a user record
/// </summary>
public class UserDao
{
    /// <summary>
    /// Unique id of the user record
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The user's display name
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The concurrency tag transportable via ETAG header.
    /// </summary>
    public required string ConcurrencyTag { get; init; }
}
