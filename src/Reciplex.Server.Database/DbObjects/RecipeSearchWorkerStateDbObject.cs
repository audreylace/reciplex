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
    nameof(Extracted),
    nameof(LeaseExpireTime),
    nameof(ExtractRetryCount),
    nameof(NextExtractRetryTime)
)]
[Index(nameof(LeaseExpireTime), nameof(DeleteRetryCounter), nameof(NextDeleteRetryTime))]
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
    public int ExtractRetryCount { get; set; }

    /// <summary>
    /// The next extract retry time
    /// </summary>
    public long? NextExtractRetryTime { get; set; }

    /// <summary>
    /// Next delete retry time
    /// </summary>
    public long? NextDeleteRetryTime { get; set; }

    /// <summary>
    /// How many delete attempts so far
    /// </summary>
    public int DeleteRetryCounter { get; set; }

    /// <summary>
    /// If the row has been extracted
    /// </summary>
    public bool Extracted { get; set; }
}
