namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

/// <summary>
/// Represents a failure
/// </summary>
public class UpdateRecipeBookDetailsFailure : IUpdateRecipeBookDetailsResults
{
    /// <summary>
    /// The failure reason
    /// </summary>
    public required UpdateRecipeBookDetailsFailureReason Reason { get; init; }

    /// <summary>
    /// Additional information that SHOULD be passed to the original caller to help them correct the failure
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}
