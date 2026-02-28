using System.Runtime.CompilerServices;

namespace Reciplex.Server.UserServices;

/// <summary>
/// Extensions for <see cref="IUserService"/>
/// </summary>
public static class UserServiceExtensions
{
    /// <summary>
    /// Fetches a set of users skipping any that fail
    /// </summary>
    /// <param name="userKeys">The key set</param>
    /// <param name="userService">user service</param>
    /// <param name="cancellationToken">async cancellation token</param>
    /// <returns>List of users that could be resolved using <paramref name="userService"/></returns>
    public static async IAsyncEnumerable<UserDao> FetchUsersAsync(
        this IUserService userService,
        IEnumerable<UserKey> userKeys,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        foreach (UserKey userKey in userKeys)
        {
            UserDao? userDao = await userService.GetUserAsync(userKey, cancellationToken);
            if (userDao is null)
            {
                continue;
            }
            yield return userDao;
        }
    }
}
