using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Extension methods for building recipe queries
/// </summary>
internal static class RecipeQueryExtensions
{
    /// <summary>
    /// Scope query to specific recipe
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    public static IQueryable<RecipeDbObject> WithRecipeId(
        this IQueryable<RecipeDbObject> query,
        long recipeId
    )
    {
        return query.Where(recipe => recipe.Id == recipeId);
    }

    /// <summary>
    /// Narrows query to only those recipes with <see cref="RecipeDbObject.Deleted"/> still null
    /// </summary>
    public static IQueryable<RecipeDbObject> DeleteFieldNull(this IQueryable<RecipeDbObject> query)
    {
        return query.Where(recipe => recipe.Deleted == null);
    }

    /// <summary>
    /// Scopes query to only recipes in a given book
    /// </summary>
    /// <param name="bookId">the book id</param>
    public static IQueryable<RecipeDbObject> ScopeToBook(
        this IQueryable<RecipeDbObject> query,
        long bookId
    )
    {
        return query.Where(recipe => recipe.RecipeBookFk == bookId);
    }
}
