namespace Reciplex.Server.Database.RecipesDomain;

public abstract record UpdateRecipeResult
{
    private UpdateRecipeResult() { }

    public record Success(RecipeDao Recipe) : UpdateRecipeResult;

    public record Forbidden() : UpdateRecipeResult;

    public record NotFound() : UpdateRecipeResult;

    public record Conflict() : UpdateRecipeResult;

    public record ValidationFailure(IDictionary<string, string[]> Errors) : UpdateRecipeResult;
}
