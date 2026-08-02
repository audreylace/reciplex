namespace Reciplex.Server.Database.Results;

/// <summary>
/// Database action blocked because the actor does not have privileges to perform the action
/// </summary>
public record class ForbiddenResult : IDatabaseResult;
