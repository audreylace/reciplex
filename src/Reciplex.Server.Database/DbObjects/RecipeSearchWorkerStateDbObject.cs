using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction record
/// </summary>
[Index(nameof(RecipeFk), nameof(SearchVersion))]
public class RecipeSearchWorkerStateDbObject
{
    /// <summary>
    /// The extracted search version
    /// </summary>
    public required long? SearchVersion { get; set; }

    /// <summary>
    /// The recipe
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public RecipeDbObject? Recipe { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeDbObject"/> for property <see cref="Recipe"/>
    /// </summary>
    public long RecipeFk { get; init; }

    /// <summary>
    /// Sole lock mechanism. Null = free to claim.
    /// </summary>
    public long? LeaseExpireTime { get; set; }

    /// <summary>
    /// The number of attempts against this record
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// The version observed at time of error
    /// </summary>
    public long? AttemptedExtractSearchVersion { get; set; }

    /// <summary>
    /// The next time retry
    /// </summary>
    public long? NextRetryTime { get; set; }

    /// <summary>
    /// The concurrency tag
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";

    /// <summary>
    /// If an extraction has ever been attempted
    /// </summary>
    public bool ExtractionAttempted { get; set; }
}
