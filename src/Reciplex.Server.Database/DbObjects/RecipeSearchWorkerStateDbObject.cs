using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction record
/// </summary>
[Index(nameof(LeaseToken), nameof(LeaseExpireTime))]
[Index(nameof(LeaseExpireTime))]
[Index(
    nameof(ExtractionStatus),
    nameof(LeaseExpireTime),
    nameof(NextRetryTime),
    nameof(RetryCount)
)]
public class RecipeSearchWorkerStateDbObject
{
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
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long RecipeFk { get; init; }

    /// <summary>
    /// The recipe book
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeBookFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeBookDbObject? Book { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeBookDbObject"/> for property <see cref="Book"/>
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long RecipeBookFk { get; init; }

    /// <summary>
    /// Sole lock mechanism. Null = free to claim.
    /// </summary>
    public long? LeaseExpireTime { get; set; }

    /// <summary>
    /// Unique string identifying the lease this record is part of.
    /// </summary>
    public string? LeaseToken { get; set; }

    /// <summary>
    /// The number of extract attempts against this record
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// The next extract retry time
    /// </summary>
    public long? NextRetryTime { get; set; }

    /// <summary>
    /// The row extraction status
    /// </summary>
    public SearchExtractionStatus ExtractionStatus { get; set; } =
        SearchExtractionStatus.PendingExtraction;

    /// <summary>
    /// Resets the tracking status for a recipe search export state object
    /// </summary>
    /// <param name="newStatus">the new status to set it to</param>
    public void ResetTrackingStatus(SearchExtractionStatus newStatus)
    {
        ExtractionStatus = newStatus;

        // reset counters
        RetryCount = 0;
        NextRetryTime = null;

        // break existing lease to invalidate an existing operation
        LeaseExpireTime = null;
        LeaseToken = null;
    }
}
