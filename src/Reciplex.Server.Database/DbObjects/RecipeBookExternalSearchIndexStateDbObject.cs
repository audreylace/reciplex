using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Record describing the state of an recipe book exported to the search index
/// </summary>
[Index(nameof(RecipeBookFk), IsUnique = true)]
[Index(nameof(SearchExtractionStartTime))]
[Index(nameof(SearchExtractionCheckTime))]
[Index(nameof(SearchExtractionStatus), nameof(SearchExtractionStartTime))]
[Index(nameof(SearchExtractionStatus), nameof(SearchExtractionCheckTime))]
public class RecipeBookExternalSearchIndexStateDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// When the record was modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// The concurrency tag for this record
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";

    /// <summary>
    /// The concurrency tag of the recipe at time of export. Use this to determine if
    /// the record has been exported and up to date or not.
    /// </summary>
    public required string ConcurrencyTagAtExport { get; set; } = "";

    /// <summary>
    /// The book holding the linked recipe
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
    /// The search extraction status
    /// </summary>
    public SearchExtractionStatus SearchExtractionStatus { get; set; }

    /// <summary>
    /// Set when the <see cref="SearchExtractionStatus"/> is first moved to <see cref="SearchExtractionStatus.InProgress"/>
    /// or the last time the extraction status was checked.
    /// </summary>
    public long? SearchExtractionCheckTime { get; set; }

    /// <summary>
    /// The time the search extraction began. Cleared when extraction is complete.
    /// </summary>
    public long? SearchExtractionStartTime { get; set; }

    /// <summary>
    /// The handle used to track search extraction
    /// </summary>
    [MaxLength(256)]
    public string? SearchExtractionHandle { get; set; }

    /// <summary>
    /// The search database handle. Use to locate the record in the search database.
    /// </summary>
    [MaxLength(256)]
    public string? SearchDatabaseHandle { get; set; }
}
