using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// A recipe held inside of a <see cref="RecipeBookDbObject"/>
/// </summary>
[Index(nameof(RecipeBookFk))]
[Index(nameof(Deleted))]
public class RecipeDbObject
{
    public const int NameMaxLength = 128;
    public const int ShortDescriptionMaxLength = 256;
    public const int DetailsMaxLength = 1024 * 1024; // 1MB

    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Name of the recipe
    /// </summary>
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Recipe short description
    /// </summary>
    [MaxLength(ShortDescriptionMaxLength)]
    public string ShortDescription { get; set; } = "";

    /// <summary>
    /// Recipe markdown details
    /// </summary>
    [MaxLength(DetailsMaxLength)]
    public string Details { get; set; } = "";

    /// <summary>
    /// When the recipe was modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the recipe was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// The concurrency tag
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";

    /// <summary>
    /// The book holding this recipe
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeBookFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeBookDbObject? RecipeBook { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeBookDbObject"/> for property <see cref="RecipeBook"/>
    /// </summary>
    public long RecipeBookFk { get; init; }

    /// <summary>
    /// Populated if the recipe is deleted. The value is the time of deletion.
    /// </summary>
    public long? Deleted { get; set; }

    /// <summary>
    /// The search version
    /// </summary>
    public required long SearchVersion { get; set; }

    /// <summary>
    /// Pointer to recipe search extraction tracking record
    /// </summary>
    public RecipeSearchExtractionStatusDbObject? RecipeSearchExtraction { get; set; }
}
