using Microsoft.AspNetCore.Mvc;

namespace Reciplex.Server.Host.Models.Authnz;

/// <summary>
/// Information about the authenticated user
/// </summary>
[ModelBinder(BinderType = typeof(UserBinder))]
public class User
{
    /// <summary>
    /// The user ID of the authenticated user
    /// </summary>
    public required long UserId { get; set; }
}
