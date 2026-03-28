namespace Reciplex.Server.Database.RecipeBooksDomain;

public abstract record CreateRecipeBookResult
{
    private CreateRecipeBookResult() { }

    public record Success(RecipeBookDao RecipeBook) : CreateRecipeBookResult;

    /// <summary>
    /// Create rejected because of validation failure
    /// </summary>
    /// <param name="Errors">validation errors</param>
    public record ValidationFailure(IDictionary<string, string[]> Errors) : CreateRecipeBookResult;
}
