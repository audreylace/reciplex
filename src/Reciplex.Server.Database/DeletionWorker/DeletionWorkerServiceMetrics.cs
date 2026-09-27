using System.Diagnostics.Metrics;

namespace Reciplex.Server.Database.DeletionWorker;

/// <summary>
/// Metrics for <see cref="DeletionWorkerService"/>
/// </summary>
internal sealed class DeletionWorkerServiceMetrics
{
    /// <summary>
    /// Label for recipes operations
    /// </summary>
    public const string RecipesVariant = "recipes";

    /// <summary>
    /// Label for user operations
    /// </summary>
    public const string UsersVariant = "users";

    /// <summary>
    /// Label for recipe book operations
    /// </summary>
    public const string RecipeBooksVariant = "recipe_books";

    /// <summary>
    /// Label for recipe book access entries operations
    /// </summary>
    public const string RecipeBookAccessEntries = "recipe_book_access_entries";

    /// <summary>
    /// Number of rows deleted broken out by their type
    /// </summary>
    private readonly Counter<long> _rowsDeletedCounter;

    /// <summary>
    /// How long an entire cleanup cycle took
    /// </summary>
    private readonly Histogram<double> _fullLoopTimeHistogram;

    /// <summary>
    /// The outcome of each row operation
    /// </summary>
    private readonly Counter<long> _operationOutcomeCounter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="meterFactory">ASP.net metric factory method</param>
    public DeletionWorkerServiceMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(
            "Reciplex.Server.Database.DeletionWorker.DeletionWorkerServiceMetrics",
            "1.0.0"
        );
        _rowsDeletedCounter = meter.CreateCounter<long>(
            "reciplex.deletion_worker.rows_deleted",
            unit: "{row}",
            description: "Total number of soft-deleted records purged."
        );

        _operationOutcomeCounter = meter.CreateCounter<long>(
            "reciplex.deletion_worker.operations",
            unit: "{operation}",
            description: "Total number of batch deletion operations executed."
        );

        _fullLoopTimeHistogram = meter.CreateHistogram<double>(
            "reciplex.deletion_worker.run_duration",
            unit: "ms",
            description: "Duration of a cleanup cycle.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    TimeSpan.FromSeconds(10).TotalMilliseconds,
                    TimeSpan.FromSeconds(30).TotalMilliseconds,
                    TimeSpan.FromMinutes(1).TotalMilliseconds,
                    TimeSpan.FromMinutes(5).TotalMilliseconds,
                    TimeSpan.FromMinutes(10).TotalMilliseconds,
                ],
            }
        );
    }

    /// <summary>
    /// Add an observation of the main loop time
    /// </summary>
    /// <param name="howLong">how long the operation took</param>
    /// <param name="success">if it was a success or not</param>
    public void ObserveMainLoop(TimeSpan howLong, bool success)
    {
        _fullLoopTimeHistogram.Record(
            howLong.TotalMilliseconds,
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure")
        );
    }

    /// <summary>
    /// Records how many rows were deleted
    /// </summary>
    /// <param name="count">the number of rows</param>
    /// <param name="rowKind">the type of row</param>
    public void IncRowsDeleted(long count, string rowKind)
    {
        if (count < 1)
        {
            return;
        }
        _rowsDeletedCounter.Add(count, new KeyValuePair<string, object?>("row_kind", rowKind));
    }

    public void IncOutcome(string rowKind, bool success)
    {
        _operationOutcomeCounter.Add(
            1,
            new KeyValuePair<string, object?>("row_kind", rowKind),
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure")
        );
    }
}
