namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Result returned from an update user request
/// </summary>
public abstract record UpdateUserResult
{
    private UpdateUserResult() { }

    /// <summary>
    /// Success result
    /// </summary>
    /// <param name="User">the new state of the record</param>
    public record Success(UserDao User) : UpdateUserResult;

    /// <summary>
    /// The concurrency tag does not match the current state of the record
    /// </summary>
    public record Conflict : UpdateUserResult;

    /// <summary>
    /// No user record found
    /// </summary>
    public record NotFound : UpdateUserResult;

    /// <summary>
    /// Update rejected because of validation failure
    /// </summary>
    /// <param name="Errors">validation errors</param>
    public record ValidationFailure(IDictionary<string, string[]> Errors) : UpdateUserResult;
}
