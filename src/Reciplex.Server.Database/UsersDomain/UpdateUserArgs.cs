namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Args for updating a user
/// </summary>
public class UpdateUserArgs
{
    /// <summary>
    /// The user display name
    /// </summary>
    public required string DisplayName { get; set; }
}
