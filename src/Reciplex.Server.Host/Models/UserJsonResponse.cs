using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host.Models;

public class UserJsonResponse
{
    public required string DisplayName { get; init; }
    public required string UserKey { get; init; }
    public required string ConcurrencyTag { get; init; }

    /// <summary>
    /// Converts a <see cref="IUserDao"/> into <see cref="UserJsonResponse"/>
    /// </summary>
    /// <param name="user">The <see cref="IUserDao"/> that will be converted into <see cref="UserJsonResponse"/></param>
    /// <returns>The resulting <see cref="UserJsonResponse"/></returns>
    [SetsRequiredMembers]
    public UserJsonResponse(UserDao user)
    {
        DisplayName = user.DisplayName;
        UserKey = user.Id;
        ConcurrencyTag = user.ConcurrencyTag;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UserJsonResponse() { }
}
