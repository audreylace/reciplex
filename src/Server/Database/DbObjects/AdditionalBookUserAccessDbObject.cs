using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Recipe.Database.DbObjects;

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
    /// User has write access. Ignored if <see cref="ReadAccess"/> is false.
    /// </summary>
    public bool WriteAccess { get; set; }

    /// <summary>
    /// User has read access. If false then <see cref="WriteAccess"/> has no meaning.
    /// </summary>
    public bool ReadAccess { get; set; }
}
