namespace Reciplex.Server.Database.Results;

/// <summary>
/// Database layer validation failed
/// </summary>
/// <param name="Errors">the validation error set</param>
public record class ValidationFailureResult(IDictionary<string, string[]> Errors) : IDatabaseResult;
