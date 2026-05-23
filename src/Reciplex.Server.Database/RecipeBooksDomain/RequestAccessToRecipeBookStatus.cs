namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Possible states of recipe book access status
/// </summary>
public enum RequestAccessToRecipeBookStatus
{
    /// <summary>
    /// not set
    /// </summary>
    Unset = 0,

    /// <summary>
    /// Request is pending
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Request is approved, the user has some level of access
    /// </summary>
    Approved = 2,

    /// <summary>
    /// No request has been sent
    /// </summary>
    NoRequestInProgress = 3,
}
