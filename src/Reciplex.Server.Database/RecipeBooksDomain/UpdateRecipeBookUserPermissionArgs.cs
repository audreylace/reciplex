namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Args for setting the permissions of a single user
/// </summary>
public class UpdateRecipeBookUserPermissionArgs
{
    /// <summary>
    /// User has access to the book. If false
    /// then all other permissions are ignored.
    /// </summary>
    public bool MayViewBook { get; set; }

    /// <summary>
    /// User can edit book information
    /// </summary>
    public bool MayEditBook { get; set; }

    /// <summary>
    /// If the owner has reviewed this entry yet.
    /// </summary>
    public bool Reviewed { get; set; }
}
