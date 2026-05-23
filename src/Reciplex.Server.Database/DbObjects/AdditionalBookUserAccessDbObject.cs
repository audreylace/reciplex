using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Controls access to a recipe book. Allows other users to access someones recipe book.
/// </summary>
[Index(nameof(UserFk), nameof(RecipeBookFk), IsUnique = true)] // clustered index to ensure one user entry per recipe book
[Index(nameof(UserFk))]
[Index(nameof(RecipeBookFk))]
public class AdditionalBookUserAccessDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// The book this is controlling access to
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeBookFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeBookDbObject? RecipeBook { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeBookDbObject"/> for property <see cref="RecipeBook"/>
    /// </summary>
    public long RecipeBookFk { get; set; }

    /// <summary>
    /// The user this entry is for
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(UserFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public UserDbObject? User { get; set; }

    /// <summary>
    /// Database FK to <see cref="UserDbObject"/> for property <see cref="User"/>
    /// </summary>
    public long UserFk { get; set; }

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
    /// If the owner has reviewed this entry yet. If false, all other permissions must be ignored.
    /// </summary>
    public bool Reviewed { get; set; }

    /// <summary>
    /// When the entry was last modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the entry was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// The concurrency tag
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";
}
