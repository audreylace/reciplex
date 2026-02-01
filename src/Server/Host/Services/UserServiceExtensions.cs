using System.Runtime.CompilerServices;

namespace Reciplex.Server.Host.Services;

/// <summary>
/// Common query patterns on application services
/// </summary>
static class UserServiceExtensions
{
    /// <summary>
    /// Fetches a set of users skipping any that fail
    /// </summary>
    /// <param name="userIdsToFetch">The key set</param>
    /// <param name="userService">user service</param>
    /// <param name="cancellationToken">async cancellation token</param>
    /// <returns>List of users that could be resolved using <paramref name="userService"/></returns>
    internal static async IAsyncEnumerable<IUserDao> FetchUsersAsync(
        this IUserService userService,
        IEnumerable<long> userIdsToFetch,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        foreach (long userId in userIdsToFetch)
        {
            IUserDao? userDao = await userService.GetUserAsync(userId, cancellationToken);
            if (userDao is null)
            {
                continue;
            }
            yield return userDao;
        }
    }
}
