namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Recipe book access permissions
/// </summary>
[Flags]
public enum RecipeBookAccessPermissions
{
    /// <summary>
    /// User has permissions to edit top level recipe book information
    /// </summary>
    EditBookInformation = 1,

    /// <summary>
    /// User has permissions to add recipes. Removing recipes and other access permissions are controlled at the recipe level.
    /// </summary>
    AddRecipes = 2,

    /// <summary>
    /// User has permission to delete the book.
    /// </summary>
    DeleteBook = 4,
}
