using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database;

namespace Reciplex.Server.UserServices;

public class UserService(ApplicationDbContext applicationDbContext) : IUserService
{
    public async Task<UserDao?> GetUserAsync(UserKey userKey, CancellationToken cancellationToken)
    {
        long userId = userKey.SurrogateKey;
        var user = await applicationDbContext
            .Users.Where(u => u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new() { Id = new(user.Id), DisplayName = user.DisplayName };
    }
}
