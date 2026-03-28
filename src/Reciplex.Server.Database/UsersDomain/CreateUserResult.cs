namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Possible results from a create user call
/// </summary>
public abstract record CreateUserResult
{
    private CreateUserResult() { }

    /// <summary>
    /// Success result
    /// </summary>
    /// <param name="User">the new record</param>
    public record Success(UserDao User) : CreateUserResult;

    /// <summary>
    /// Update rejected because of validation failure
    /// </summary>
    /// <param name="Errors">validation errors</param>
    public record ValidationFailure(IDictionary<string, string[]> Errors) : CreateUserResult;
}
