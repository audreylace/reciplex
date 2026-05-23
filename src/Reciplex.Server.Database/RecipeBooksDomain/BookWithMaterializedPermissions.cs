using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// book object with additional permissions and access entry if relevant
/// </summary>
/// <param name="Book">book DB object</param>
/// <param name="PermissionFlags">Computed permission flags</param>
/// <param name="AccessEntry">the share entry if one exists</param>
sealed record class BookWithMaterializedPermissions(
    RecipeBookDbObject Book,
    BookPermissionFlags PermissionFlags,
    AdditionalBookUserAccessDbObject? AccessEntry
);
