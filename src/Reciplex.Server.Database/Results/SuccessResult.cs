namespace Reciplex.Server.Database.Results;

/// <summary>
/// Success result with a value from database
/// </summary>
/// <typeparam name="TValue">the type of the value</typeparam>
/// <param name="Value">the value from the database</param>
public record class SuccessResult<TValue>(TValue Value) : IDatabaseResult;
