namespace Reciplex.Server.Database.Results;

/// <summary>
/// User not found
/// </summary>
/// <param name="UserKey">the user key</param>
public record class UserNotFoundResult(string UserKey) : IDatabaseResult;
