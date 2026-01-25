namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;

/// <summary>
/// The results of a create recipe book operation
/// </summary>
public enum CreateRecipeBookResultOutcome
{
    /// <summary>
    /// Book created
    /// </summary>
    Success,

    /// <summary>
    /// Book creation rejected because inputs were bad
    /// </summary>
    ValidationErrors,

    /// <summary>
    /// Book creation failed because of an internal error
    /// </summary>
    InternalError,

    /// <summary>
    /// Book creation failed because the user does not have required permissions
    /// </summary>
    PermissionFailure,
}
