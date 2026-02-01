namespace Reciplex.Server.Host.Services;

public interface IUserService
{
    Task<IUserDao?> GetUserAsync(long userId, CancellationToken cancellationToken);
}
