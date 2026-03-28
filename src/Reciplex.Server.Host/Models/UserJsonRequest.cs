namespace Reciplex.Server.Host.Models;

/// <summary>
/// Json body for a create user post request
/// </summary>
public class UserJsonRequest
{
    /// <summary>
    /// The display name of the user
    /// </summary>
    public required string DisplayName { get; init; }
}
