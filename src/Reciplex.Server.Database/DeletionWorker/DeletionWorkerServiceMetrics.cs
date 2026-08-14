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
    /// Tracks if the loop is running or sleeping
    /// </summary>
    private readonly Gauge<long> _running;

    /// <summary>
    /// Next time garbage collection will run
    /// </summary>
    private readonly Gauge<long> _nextRunTime;

    /// <summary>
    /// History of how long each query took
    /// </summary>
    private readonly Histogram<double> _queryTimeHistogram;

    /// <summary>
    /// History of how long each cleanup unit takes. A cleanup unit is find X of rows
    /// and then run the operation on the X rows.
    /// </summary>
    private readonly Histogram<double> _operationHistogram;

    /// <summary>
    /// Provider for getting timestamps
    /// </summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// How long an entire cleanup cycle took
    /// </summary>
    private readonly Histogram<double> _fullLoopTimeHistogram;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="meterFactory">ASP.net metric factory method</param>
    public DeletionWorkerServiceMetrics(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        Meter meter = meterFactory.Create(
            "Reciplex.Server.Database.DeletionWorker.DeletionWorkerServiceMetrics",
            "1.0.0"
        );
        _rowsDeletedCounter = meter.CreateCounter<long>(
            "reciplex.deletion_worker.rows_deleted",
            unit: "rows",
            description: "Total number of soft-deleted records purged."
        );

        _running = meter.CreateGauge<long>(
            "reciplex.deletion_worker.running_gauge",
            unit: "boolean",
            description: "Has a value of 1 when the main deletion loop is running. 0 when the main loop is sleeping."
        );

        _operationHistogram = meter.CreateHistogram<double>(
            "reciplex.deletion_worker.operation_duration",
            unit: "operations",
            description: "Total number of operations, how long they took, and their outcomes.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [1, 10, 100, 1000, 2000, 4000, 8000, 16000],
            }
        );

        _nextRunTime = meter.CreateGauge<long>(
            "reciplex.deletion_worker.next_run_time",
            unit: "timestamp",
            description: "The timestamp for when the next run is expected to occur."
        );

        _timeProvider = timeProvider;

        _queryTimeHistogram = meter.CreateHistogram<double>(
            "reciplex.deletion_worker.query_duration",
            unit: "ms",
            description: "Execution time of the database query.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [1, 10, 100, 1000, 2000, 4000, 8000, 16000],
            }
        );

        _fullLoopTimeHistogram = meter.CreateHistogram<double>(
            "reciplex.deletion_worker.cleanup_cycle_duration",
            unit: "ms",
            description: "How long a full cleanup cycle took.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [4000, 8000, 16000, 32000, 64000, 128000, 256000],
            }
        );
    }

    /// <summary>
    /// Add an observation of the main loop sleep time
    /// </summary>
    /// <param name="minutesTillNextRun">the number of minutes</param>
    public void ObserveMainLoop(double minutesTillNextRun, double msForRunTime)
    {
        _nextRunTime.Record(
            _timeProvider.GetUtcNow().AddMinutes(minutesTillNextRun).ToUnixTimeSeconds()
        );
        _fullLoopTimeHistogram.Record(msForRunTime);
    }

    private void ObserveQueryTime(double seconds, string variant, string query)
    {
        _queryTimeHistogram.Record(
            seconds,
            new KeyValuePair<string, object?>("rows", variant),
            new KeyValuePair<string, object?>("query", query)
        );
    }

    /// <summary>
    /// Sets the running status of the main loop
    /// </summary>
    /// <param name="isRunning">if its running or not</param>
    public void SetRunningStatus(bool isRunning)
    {
        _running.Record(isRunning ? 1 : 0);
    }

    /// <summary>
    /// Records an operation outcome
    /// </summary>
    /// <param name="success">if the operation was a success</param>
    /// <param name="variant">the type of row</param>
    /// <param name="duration">how long the operation took</param>
    public void RecordOperationOutcome(bool success, string variant, double duration)
    {
        _operationHistogram.Record(
            duration,
            new KeyValuePair<string, object?>("rows", variant),
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure")
        );
    }

    /// <summary>
    /// Records a find rows to delete query observation
    /// </summary>
    /// <param name="variant">the type</param>
    /// <param name="time">the time in seconds the query took</param>
    public void ObserveDeletionCandidateQuery(string variant, double time)
    {
        ObserveQueryTime(time, variant, "find_rows_to_delete");
    }

    /// <summary>
    /// Records a deletion query observation
    /// </summary>
    /// <param name="variant">the type</param>
    /// <param name="time">the time in seconds the query took</param>
    public void ObserveDeletionQuery(string variant, double time)
    {
        ObserveQueryTime(time, variant, "delete_rows");
    }

    /// <summary>
    /// Records how many rows were deleted
    /// </summary>
    /// <param name="count">the number of rows</param>
    /// <param name="variant">the type of row</param>
    public void IncRowsDeleted(long count, string variant)
    {
        if (count < 1)
        {
            return;
        }
        _rowsDeletedCounter.Add(count, new KeyValuePair<string, object?>("rows", variant));
    }
}
