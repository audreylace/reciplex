using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Change entry queue
/// </summary>
[Index(nameof(TargetRecipeBookFk))]
[Index(nameof(TargetRecipeFk))]
[Index(nameof(Created), nameof(RecordChangeKind))]
public class RecordDbObjectChangeEntry
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// When this entry was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// The record kind
    /// </summary>
    public RecordChangeSourceKind RecordKind { get; init; }

    /// <summary>
    /// The source event kind
    /// </summary>
    public required RecordChangeActionKind ChangeKind { get; init; }

    /// <summary>
    /// The recipe this entry targets
    /// </summary>
    [ForeignKey(nameof(TargetRecipeFk))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public RecipeDbObject? TargetRecipe { get; set; }

    /// <summary>
    /// The FK of the targeted recipe
    /// </summary>
    public long? TargetRecipeFk { get; set; }

    /// <summary>
    /// The recipe book this targets
    /// </summary>
    [ForeignKey(nameof(TargetRecipeBookFk))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public RecipeBookDbObject? TargetRecipeBook { get; set; }

    /// <summary>
    /// The FK of the targeted recipe book
    /// </summary>
    public long? TargetRecipeBookFk { get; set; }

    /// <summary>
    /// The observed concurrency tag of the record at queue creation.
    /// </summary>
    public required string ObservedSearchVersionTag { get; init; }
}
