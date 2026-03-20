namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Encodes the result of the delete recipe book
/// </summary>
public abstract record DeleteRecipeBookResult
{
    private DeleteRecipeBookResult() { }

    public record Success() : DeleteRecipeBookResult;

    public record Conflict() : DeleteRecipeBookResult;

    public record Forbidden() : DeleteRecipeBookResult;

    public record NotFound() : DeleteRecipeBookResult;
}
