using System.Diagnostics.Metrics;

namespace Reciplex.Server.Database.SearchExporter;

class SearchExporterMetrics
{
    #region Constants
    public const string RowsExportRetryFailed = "export_retry_failed";
    public const string RowsExportRetrySuccess = "export_retry_success";
    public const string RowsExportSuccess = "export_success";
    public const string RowsExportDatabaseErrorSuccess = "export_failed_to_mark_extracted";
    public const string RowsExportDatabaseErrorFailure = "export_failed_to_increment_retry_counter";
    public const string RecipesKind = "recipes";
    public const string CreatedIndexOperationOutcomeKind = "created";
    public const string ExistsIndexOperationOutcomeKind = "exists";
    public const string ErrorIndexOperationOutcomeKind = "error";

    #endregion Constants

    /// <summary>
    /// History of how long each query took
    /// </summary>
    private readonly Histogram<double> _queryTimeHistogram;

    /// <summary>
    /// Counts tracking the number of rows
    /// </summary>
    private readonly Counter<long> _rowsCounter;

    /// <summary>
    /// Histogram for how long each operation took
    /// </summary>
    private readonly Histogram<double> _operationHistogram;

    private readonly Histogram<double> _searchIndexMutationTime;
    private readonly Histogram<double> _searchIndexExportTime;

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

        _rowsExportedToSearchIndexCounter = meter.CreateCounter<long>(
            "reciplex.search_exporter.rows_exported_to_search_index_counter",
            unit: "rows",
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

        _emptySearchExtractionCreationDuration = meter.CreateHistogram<double>(
            "reciplex.search_exporter.empty_search_rows_creation_duration",
            unit: "time",
            description: "Total time it took to create empty search rows.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [100, 500, 1000, 2000, 4000, 8000],
            }
        );

        _emptySearchRowsCreatedCounter = meter.CreateCounter<long>(
            "reciplex.search_exporter.empty_search_rows_created_counter",
            unit: "rows",
            description: "Total number of empty search rows created."
        );

        _rowsCounter = meter.CreateCounter<long>(
            "reciplex.search_exporter.rows_counter",
            unit: "rows",
            description: "Counts number of rows based on the row type (rows) and the operation (operation_kind)."
        );

        _queryTimeHistogram = meter.CreateHistogram<double>(
            "reciplex.search_exporter.query_duration",
            unit: "ms",
            description: "Execution time of the database query.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [1, 10, 100, 1000, 2000, 4000, 8000, 16000],
            }
        );

        _operationHistogram = meter.CreateHistogram<double>(
            "reciplex.search_exporter.operation_duration",
            unit: "operations",
            description: "Total number of operations, how long they took, and their outcomes.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [1, 10, 100, 1000, 2000, 4000, 8000, 16000],
            }
        );
    }

    /// <summary>
    /// Records the time and outcome of a recipe index operation
    /// </summary>
    /// <param name="kind">the index kind</param>
    /// <param name="indexOutcomeKind">the outcome of the operation</param>
    /// <param name="duration">the duration in milliseconds</param>
    public void ObserveRecordSearchIndexUpsert(
        string kind,
        string indexOutcomeKind,
        double duration
    )
    {
        _searchIndexMutationTime.Record(
            duration,
            new KeyValuePair<string, object?>("index", kind),
            new KeyValuePair<string, object?>("outcome", indexOutcomeKind)
        );
    }

    public void ObserveRecipesExported(string kind, long rows, string outcome)
    {
        _rowsCounter.Add(
            rows,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("operation_kind", outcome)
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

    public void ObserveEmptyRowCreationDuration(string kind, bool success, double duration)
    {
        _operationHistogram.Record(
            duration,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("operation_kind", "empty_row_creation")
        );
    }

    public void ObserveEmptyRowDeletionDuration(string kind, bool success, double duration)
    {
        _operationHistogram.Record(
            duration,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("outcome", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("operation_kind", "empty_row_deletion")
        );
    }

    public void ObserveEmptyRowCreation(string kind, long rows)
    {
        _rowsCounter.Add(
            rows,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("operation_kind", "empty_row_creation")
        );
    }

    /// <summary>
    /// Records metrics for empty search row deletion
    /// </summary>
    /// <param name="kind">the data kind</param>
    /// <param name="rows">the number of rows deleted</param>
    /// <param name="collectTime">how long the query took to find rows</param>
    /// <param name="deletionTime">how long the query took to delete rows</param>
    public void ObserveEmptySearchRowDeletion(
        string kind,
        long rows,
        double collectTime,
        double deletionTime
    )
    {
        _rowsCounter.Add(
            rows,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("operation_kind", "empty_row_deletion")
        );

        _queryTimeHistogram.Record(
            collectTime,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("query", "find_empty_rows_to_delete")
        );

        _queryTimeHistogram.Record(
            deletionTime,
            new KeyValuePair<string, object?>("rows", kind),
            new KeyValuePair<string, object?>("query", "delete_empty_rows")
        );
    }
}
