namespace Reciplex.Server.Database.RecipesDomain;

public abstract record CreateRecipeResult
{
    private CreateRecipeResult() { }

    public record Success(RecipeDao Recipe) : CreateRecipeResult;

    public record Forbidden() : CreateRecipeResult;

    public record NotFound() : CreateRecipeResult;

    public record ValidationFailure(IDictionary<string, string[]> Errors) : CreateRecipeResult;
}
