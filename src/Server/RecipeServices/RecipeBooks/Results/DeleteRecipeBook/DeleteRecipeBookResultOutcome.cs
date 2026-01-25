namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;

/// <summary>
/// Possible outcomes from a delete operation
/// </summary>
public enum DeleteRecipeBookResultOutcome
{
    /// <summary>
    /// Recipe book was deleted
    /// </summary>
    Success,

    /// <summary>
    /// User does not have delete access
    /// </summary>
    DoesNotHaveAccess,

    /// <summary>
    /// Concurrency conflict
    /// </summary>
    ConcurrencyConflict,

    /// <summary>
    /// Book does not exist or the user does not have any permissions to know the book exists
    /// </summary>
    NotFound,

    /// <summary>
    /// Some other error
    /// </summary>
    InternalError,
}
