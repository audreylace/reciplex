namespace Reciplex.Server.Host.Services;

public interface IUserService
{
    Task<IUserDao?> GetUserAsync(string userId, CancellationToken cancellationToken);
}
