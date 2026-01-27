namespace Reciplex.Server.RecipeServices.Recipes.Models;

/// <summary>
/// Recipe access permissions
/// </summary>
[Flags]
public enum RecipeAccessPermissions
{
    /// <summary>
    /// User can delete recipe
    /// </summary>
    DeleteRecipe = 1,

    /// <summary>
    /// User can edit recipe
    /// </summary>
    EditRecipe = 2,
}
