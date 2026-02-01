using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// A recipe held inside of a <see cref="RecipeBookDbObject"/>
/// </summary>
[Index(nameof(RecipeBookFk))]
public class RecipeDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Name of the recipe
    /// </summary>
    [MaxLength(127)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Recipe short description
    /// </summary>
    [MaxLength(255)]
    public string ShortDescription { get; set; } = "";

    /// <summary>
    /// Recipe markdown details
    /// </summary>
    [MaxLength(1024 * 1024)]
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
    /// The stored json model
    /// </summary>
    public RecipeDbJsonObject JsonData { get; set; } = new();

    /// <summary>
    /// True if the recipe is deleted
    /// </summary>
    public bool Deleted { get; set; }
}
