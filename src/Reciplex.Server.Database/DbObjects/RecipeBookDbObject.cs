using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Recipe book database object
/// </summary>
[Index(nameof(OwnerFk))]
[Index(nameof(Deleted))]
public class RecipeBookDbObject
{
    public const int NameMaxLength = 128;
    public const int ShortDescriptionMaxLength = 256;

    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Title of the recipe book
    /// </summary>
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Recipe book short description
    /// </summary>
    [MaxLength(ShortDescriptionMaxLength)]
    public string ShortDescription { get; set; } = "";

    /// <summary>
    /// When the recipe book was modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the recipe book was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// The concurrency tag
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";

    /// <summary>
    /// Owner of this book
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(OwnerFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public UserDbObject? Owner { get; set; }

    /// <summary>
    /// Database FK to <see cref="UserDbObject"/> for property <see cref="Owner"/>
    /// </summary>
    public long OwnerFk { get; set; }

    /// <summary>
    /// Navigation property to recipes in this book
    /// </summary>
    public ICollection<RecipeDbObject> Recipes { get; init; } = [];

    /// <summary>
    /// Users with additional access
    /// </summary>
    public ICollection<AdditionalBookUserAccessDbObject> AdditionalUsers { get; init; } = [];

    /// <summary>
    /// The recipe book share key. If an empty string, then sharing is disabled.
    /// </summary>
    public string ShareKey { get; set; } = "";

    /// <summary>
    /// Populated if the recipe book is deleted. The value is the time of deletion.
    /// </summary>
    public long? Deleted { get; set; }
}
