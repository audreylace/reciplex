namespace Reciplex.Server.Database.Results;

/// <summary>
/// Return when a book was not found
/// </summary>
/// <param name="BookId">the book id that was not found</param>
public record class BookNotFoundResult(string BookId) : IDatabaseResult;
