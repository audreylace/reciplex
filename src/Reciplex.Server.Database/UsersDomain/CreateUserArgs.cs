namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Args for creating a user
/// </summary>
public class CreateUserArgs
{
    /// <summary>
    /// The user display name
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// The OIDC Authority
    /// </summary>
    public required string Authority { get; set; }

    /// <summary>
    /// The OIDC Subject
    /// </summary>
    public required string Subject { get; set; }
}
