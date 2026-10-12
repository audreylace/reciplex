using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Recipe book database object
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Index on <see cref="OwnerFk"/></item>
/// <item>Index on ( <see cref="IsDeleted"/>, <see cref="NextDeletePoll"/> ) </item>
/// </list>
/// </remarks>
[Index(nameof(OwnerFk))]
[Index(nameof(IsDeleted), nameof(NextDeletePoll))]
public class RecipeBookDbObject
{
    /// <summary>
    /// Max name length: <see cref="Name"/>
    /// </summary>
    public const int NameMaxLength = 128;

    /// <summary>
    /// Max length for short description: <see cref="ShortDescription"/>
    /// </summary>
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
    /// If the book is deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp of deletion
    /// </summary>
    public long? DeletedTime { get; set; }

    /// <summary>
    /// Next time to check for complete deletion of related entities
    /// </summary>
    public long NextDeletePoll { get; set; }

    /// <summary>
    /// Counter used to compute the backoff for <see cref="NextDeletePoll"/>
    /// </summary>
    public long NextDeletePollCounter { get; set; }
}
