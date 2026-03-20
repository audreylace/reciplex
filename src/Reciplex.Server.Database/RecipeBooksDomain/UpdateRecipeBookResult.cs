namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Result of a update recipe result
/// </summary>
public abstract record UpdateRecipeBookResult
{
    private UpdateRecipeBookResult() { }

    public record Success(RecipeBookDao RecipeBook) : UpdateRecipeBookResult;

    public record Conflict() : UpdateRecipeBookResult;

    public record Forbidden() : UpdateRecipeBookResult;

    public record NotFound() : UpdateRecipeBookResult;

    /// <summary>
    /// Create rejected because of validation failure
    /// </summary>
    /// <param name="Errors">validation errors</param>
    public record ValidationFailure(IDictionary<string, string[]> Errors) : UpdateRecipeBookResult;
}
