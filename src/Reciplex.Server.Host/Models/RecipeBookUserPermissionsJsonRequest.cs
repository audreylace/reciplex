namespace Reciplex.Server.Host.Models;

/// <summary>
/// Permission object to modify a user
/// </summary>
public class RecipeBookUserPermissionsJsonRequest
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
