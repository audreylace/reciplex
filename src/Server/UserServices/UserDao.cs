namespace Reciplex.Server.UserServices;

public class UserDao
{
    public required UserKey Id { get; init; }
    public required string DisplayName { get; init; }
}
