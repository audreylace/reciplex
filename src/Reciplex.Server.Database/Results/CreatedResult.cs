namespace Reciplex.Server.Database.Results;

/// <summary>
/// Created result from database with a value
/// </summary>
/// <typeparam name="TValue">the type of the value</typeparam>
/// <param name="Value">the value from the database</param>
public record class CreatedResult<TValue>(TValue Value) : IDatabaseResult;
