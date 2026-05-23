namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Medium to hold book permissions flags. Used to pass
/// information between methods internally.
/// </summary>
readonly struct BookPermissionFlags
{
    /// <summary>
    /// User can edit book information
    /// </summary>
    public required bool MayEditBook { get; init; }

    /// <summary>
    /// User can delete book
    /// </summary>
    public required bool MayDeleteBook { get; init; }

    /// <summary>
    /// If the user can share the book
    /// </summary>
    public required bool MayShareBook { get; init; }

    /// <summary>
    /// If the user can manage book access
    /// </summary>
    public required bool MayManageAccess { get; init; }

    /// <summary>
    /// If the user can view the book
    /// </summary>
    public required bool MayViewBook { get; init; }

    /// <summary>
    /// If the user owns the book
    /// </summary>
    public required bool OwnsBook { get; init; }

    /// <summary>
    /// Configures the permissions on <see cref="BookPermissionFlags"/> for a user
    /// who owns the book.
    /// </summary>
    /// <returns><see cref="BookPermissionFlags"/> with flags set</returns>
    public static BookPermissionFlags OwnerPermissions()
    {
        return new()
        {
            MayEditBook = true,
            MayDeleteBook = true,
            MayShareBook = true,
            MayManageAccess = true,
            OwnsBook = true,
            MayViewBook = true,
        };
    }

    /// <summary>
    /// Configures the permissions on <see cref="BookPermissionFlags"/> for a user
    /// who does not owns the book but has view access and possibly edit access.
    /// </summary>
    /// <param name="mayEdit">if the user has edit access</param>
    /// <returns><see cref="BookPermissionFlags"/> with flags set</returns>
    public static BookPermissionFlags SharedPermissions(bool mayEdit)
    {
        return new()
        {
            MayEditBook = mayEdit,
            MayDeleteBook = false,
            MayShareBook = false,
            MayManageAccess = false,
            OwnsBook = false,
            MayViewBook = true,
        };
    }

    /// <summary>
    /// Configures the permissions on <see cref="BookPermissionFlags"/> for a user
    /// who does not have any access
    /// </summary>
    /// <returns><see cref="BookPermissionFlags"/> with flags set</returns>
    public static BookPermissionFlags NoAccess()
    {
        return new()
        {
            MayEditBook = false,
            MayDeleteBook = false,
            MayShareBook = false,
            MayManageAccess = false,
            OwnsBook = false,
            MayViewBook = false,
        };
    }
}
