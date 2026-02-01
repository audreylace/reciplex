using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.Models.User;

public class UserJson
{
    public required string DisplayName { get; init; }
    public required UserKey UserId { get; init; }

    /// <summary>
    /// Converts a <see cref="IUserDao"/> into <see cref="UserJson"/>
    /// </summary>
    /// <param name="user">The <see cref="IUserDao"/> that will be converted into <see cref="UserJson"/></param>
    /// <returns>The resulting <see cref="UserJson"/></returns>
    [SetsRequiredMembers]
    public UserJson(UserDao user)
    {
        DisplayName = user.DisplayName;
        UserId = user.Id;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UserJson() { }
}
