namespace Reciplex.Server.Database.Results;

/// <summary>
/// User not found
/// </summary>
/// <param name="UserId">the user key</param>
public record class UserNotFoundResult(long UserId) : IDatabaseResult;
