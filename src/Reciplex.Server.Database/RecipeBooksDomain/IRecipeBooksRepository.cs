namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Service providing access to recipe books
/// </summary>
public interface IRecipeBooksRepository
{
    /// <summary>
    /// Gets a recipe book by its ID
    /// </summary>
    /// <param name="bookId">The id of the recipe book</param>
    /// <param name="userKey">The user request access</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task holding the book if it exists</returns>
    Task<RecipeBookDao?> GetRecipeBookAsync(
        string bookId,
        string userKey,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates a recipe book
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="userKey">The id of the user performing the action for access checks</param>
    /// <param name="args">Arguments to the update transporting the new state of the book</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<UpdateRecipeBookResult> UpdateRecipeBookAsync(
        string bookId,
        string userKey,
        UpdateRecipeBookArgs args,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Deletes a recipe book
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="userKey">The key of the user performing the action for access checks</param>
    /// <param name="concurrencyTag">concurrency token of the book to ensure the user is deleted the one they observed</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<DeleteRecipeBookResult> DeleteRecipeBookAsync(
        string bookId,
        string userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    );

    Task<CreateRecipeBookResult> CreateRecipeBookAsync(
        string userKey,
        CreateRecipeBookArgs args,
        CancellationToken cancellationToken
    );

    IAsyncEnumerable<RecipeBookDao> ListRecipeBooksAsync(
        string userKey,
        ListRecipeBooksArgs args,
        CancellationToken cancellationToken
    );
}
