using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

abstract record RecipeDbObjectQueryResult
{
    private RecipeDbObjectQueryResult() { }

    public record NotFound() : RecipeDbObjectQueryResult;

    public record Forbidden() : RecipeDbObjectQueryResult;

    public record Success(RecipeDbObject Recipe, RecipeBookDbObject Book, bool MayEdit)
        : RecipeDbObjectQueryResult;
}
