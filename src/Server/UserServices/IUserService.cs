namespace Reciplex.Server.UserServices;

public interface IUserService
{
    Task<UserDao?> GetUserAsync(UserKey userKey, CancellationToken cancellationToken);
}
