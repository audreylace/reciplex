using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction record for a recipe or recipe book
/// </summary>
/// <remarks>
/// <list>
/// <item>Has index on ( <see cref="LeaseToken"/>, <see cref="LeaseExpireToken"/> )</item>
/// <item>Has index on <see cref="LeaseExpireTime"/></item>
/// <item>Has index on ( <see cref="ExtractionStatus"/>, <see cref="LeaseExpireTime"/>, <see cref="NextRetryTime"/>, <see cref="RetryCount"/> )</item>
/// <item>Has index on ( <see cref="RecipeFk"/>, <see cref="ExtractionStatus"/> )</item>
/// <item>Has index on ( <see cref="RecipeBookFk"/>, <see cref="ExtractionStatus"/> )</item>
/// </list>
/// </remarks>
[Index(nameof(LeaseToken), nameof(LeaseExpireTime))]
[Index(nameof(LeaseExpireTime))]
[Index(
    nameof(ExtractionStatus),
    nameof(LeaseExpireTime),
    nameof(NextRetryTime),
    nameof(RetryCount)
)]
[Index(nameof(RecipeFk))]
[Index(nameof(RecipeBookFk))]
[Index(nameof(ExtractionStatus), nameof(RetryCount))]
public class SearchExtractionStatusDbObject
{
    /// <summary>
    /// Row database primary id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// The recipe
    /// </summary>
    [ForeignKey(nameof(RecipeFk))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeDbObject? Recipe { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeDbObject"/> for property <see cref="Recipe"/>
    /// </summary>
    public long? RecipeFk { get; init; }

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
    /// The row kind
    /// </summary>
    public required SearchExportRowKind RowKind { get; set; }

    /// <summary>
    /// Resets the tracking status of this instance
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
