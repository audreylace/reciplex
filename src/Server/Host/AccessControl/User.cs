using Microsoft.AspNetCore.Mvc;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Information about the authenticated user
/// </summary>
[ModelBinder(BinderType = typeof(UserBinder))]
public class User
{
    /// <summary>
    /// The user ID of the authenticated user
    /// </summary>
    public required string UserId { get; set; }
}
