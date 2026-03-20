namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Possible results from a delete user call
/// </summary>
public abstract record DeleteUserResult
{
    private DeleteUserResult() { }

    /// <summary>
    /// Delete was a success. Record either did not exist before this
    /// request was executed or is now deleted.
    /// </summary>
    public record Success : DeleteUserResult;

    /// <summary>
    /// The concurrency tag does not match the current state of the record
    /// </summary>
    public record Conflict : DeleteUserResult;
}
