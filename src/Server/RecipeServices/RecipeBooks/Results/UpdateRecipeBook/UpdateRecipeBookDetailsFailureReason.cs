namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

/// <summary>
/// Various failure reasons for a recipe book update
/// </summary>
public enum UpdateRecipeBookDetailsFailureReason
{
    /// <summary>
    /// User does not have write access
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
    /// Argument validation failure
    /// </summary>
    ValidationFailure,

    /// <summary>
    /// Some other error
    /// </summary>
    InternalError,
}
