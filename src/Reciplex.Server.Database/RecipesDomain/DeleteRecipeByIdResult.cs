namespace Reciplex.Server.Database.RecipesDomain;

public abstract record DeleteRecipeByIdResult
{
    private DeleteRecipeByIdResult() { }

    public record Success() : DeleteRecipeByIdResult;

    public record Forbidden() : DeleteRecipeByIdResult;

    public record NotFound() : DeleteRecipeByIdResult;

    public record Conflict() : DeleteRecipeByIdResult;
}
