using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

namespace Reciplex.Server.RecipeServices.RecipeBooks;

/// <summary>
/// Service providing access to recipe books
/// </summary>
public interface IRecipeBookService
{
    /// <summary>
    /// Gets a recipe book by its ID
    /// </summary>
    /// <param name="bookId">The id of the recipe book</param>
    /// <param name="userId">The user request access</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task holding the book if it exists</returns>
    Task<RecipeBookDao?> GetRecipeBookAsync(
        string bookId,
        string userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates a recipe book
    /// </summary>
    /// <param name="bookId">The string id of the book</param>
    /// <param name="userId">The string id of the user performing the action for access checks</param>
    /// <param name="args">Arguments to the update transporting the new state of the book</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<IUpdateRecipeBookDetailsResults> UpdateRecipeBookDetailsAsync(
        string bookId,
        string userId,
        UpdateRecipeBookDetailsArgs args,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Deletes a recipe book
    /// </summary>
    /// <param name="bookId">The string id of the book</param>
    /// <param name="userId">The string id of the user performing the action for access checks</param>
    /// <param name="concurrencyTag">concurrency token of the book to ensure the user is deleted the one they observed</param>
    /// <param name="cancellationToken">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<DeleteRecipeBookResult> DeleteRecipeBookAsync(
        string bookId,
        string userId,
        string concurrencyTag,
        CancellationToken cancellationToken
    );

    Task<CreateRecipeBookResult> CreateRecipeBookAsync(
        string userId,
        CreateRecipeBookArgs args,
        CancellationToken cancellationToken
    );

    Task<IAsyncEnumerable<RecipeBookDao>?> ListRecipeBooksAsync(
        string userId,
        ListRecipeBooksArgs args,
        CancellationToken cancellationToken
    );
}
