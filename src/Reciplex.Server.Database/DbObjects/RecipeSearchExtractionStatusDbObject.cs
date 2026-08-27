using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction record
/// </summary>
[Index(nameof(RecipeFk))]
[Index(nameof(RecipeFk), nameof(SearchVersion))]
[Index(nameof(TaskUid))]
public class RecipeSearchExtractionStatusDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// The extracted search version
    /// </summary>
    public required long? SearchVersion { get; set; }

    /// <summary>
    /// The book owning the recipe
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
    /// The recipe
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeDbObject? Recipe { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeDbObject"/> for property <see cref="Recipe"/>
    /// </summary>
    public long RecipeFk { get; init; }

    /// <summary>
    /// The batch id in the external search system
    /// </summary>
    public long? TaskUid { get; set; }

    /// <summary>
    /// The time the batch id was posted
    /// </summary>
    public long? TaskPostTime { get; set; }

    /// <summary>
    /// The number of attempts to extract this record
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// The version observed at time of error
    /// </summary>
    public long? ErrorSearchVersion { get; set; }
}
