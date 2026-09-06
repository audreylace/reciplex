using System.Diagnostics.Metrics;

namespace Reciplex.Server.Database.SearchExporter;

class SearchExporterMetrics
{
    private readonly Histogram<double> _searchIndexMutationTime;
    private readonly Histogram<double> _searchIndexExportTime;
    private readonly Counter<long> _rowsExported;

    public SearchExporterMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(
            "Reciplex.Server.Database.SearchExporter.SearchExporterMetrics",
            "1.0.0"
        );

        _searchIndexMutationTime = meter.CreateHistogram<double>(
            "reciplex.search_exporter.search_index_upsert_duration",
            unit: "operations",
            description: "Total number of index creation/upsert and how long they took",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [50, 100, 1000, 2000],
            }
        );

        _rowsExported = meter.CreateCounter<long>(
            "reciplex.search_exporter.rows_exported",
            unit: "count",
            description: "Total number of rows exported to the search index tagged by the operation kind and outcome."
        );

        _searchIndexExportTime = meter.CreateHistogram<double>(
            "reciplex.search_exporter.search_index_total_operation_duration",
            unit: "operations",
            description: "Total time an extraction operation took from start to completion.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [100, 500, 1000, 2000, 4000, 8000],
            }
        );
    }

    public const string RecipesKind = "recipes";
    public const string CreatedIndexOperationOutcomeKind = "created";
    public const string ExistsIndexOperationOutcomeKind = "exists";
    public const string ErrorIndexOperationOutcomeKind = "error";

    /// <summary>
    /// Records the time and outcome of a recipe index operation
    /// </summary>
    /// <param name="kind">the index kind</param>
    /// <param name="indexOutcomeKind">the outcome of the operation</param>
    /// <param name="duration">the duration in milliseconds</param>
    public void RecordSearchIndexUpsert(string kind, string indexOutcomeKind, double duration)
    {
        _searchIndexMutationTime.Record(
            duration,
            new KeyValuePair<string, object?>("index", kind),
            new KeyValuePair<string, object?>("outcome", indexOutcomeKind)
        );
    }

    public const string RowsExportRetryFailed = "retry_failed";
    public const string RowsExportRetrySuccess = "retry_success";
    public const string RowsExportSuccess = "success";
    public const string RowsExportDatabaseErrorSuccess = "failed_to_mark_extracted";
    public const string RowsExportDatabaseErrorFailure = "failed_to_increment_retry_counter";

    public void IncrementRowsExportedCounter(string kind, long rows, string outcome)
    {
        _rowsExported.Add(
            rows,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("outcome", outcome)
        );
    }

    public void ObserveSearchExportOperation(string kind, bool success, double duration)
    {
        _searchIndexExportTime.Record(
            duration,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure")
        );
    }
}
