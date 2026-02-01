namespace Reciplex.Server.RecipeServices.Recipes.Models;

/// <summary>
/// Recipe access permissions
/// </summary>
[Flags]
public enum RecipeAccessPermissions
{
    Unset = 0,

    /// <summary>
    /// User can read recipe
    /// </summary>
    Read = 1,

    /// <summary>
    /// User can delete recipe
    /// </summary>
    DeleteRecipe = 2,

    /// <summary>
    /// User can edit recipe
    /// </summary>
    EditRecipe = 4,
}
