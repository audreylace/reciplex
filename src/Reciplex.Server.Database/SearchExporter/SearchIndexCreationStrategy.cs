using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Strategy for creating search indexes for the application if they do not already exist
/// </summary>
class SearchIndexCreationStrategy
{
    #region Constants

    /// <summary>
    /// Recipe search index
    /// </summary>
    private const string RecipesSearchIndex = "recipes";

    /// <summary>
    /// The primary key property name
    /// </summary>
    private const string PrimaryKeyPropertyName = "id";

    #endregion Constants

    /// <summary>
    /// The search client
    /// </summary>
    readonly IMeilisearchClient _searchClient;

    /// <summary>
    /// the logger for this instance
    /// </summary>
    readonly ILogger<SearchIndexCreationStrategy> _logger;

    /// <summary>
    /// Metric tracking how long the search index query and creation operation took
    /// </summary>
    private readonly Histogram<double> _searchIndexUpsertTime;

    /// <summary>
    /// If the recipe index already exists
    /// </summary>
    private bool _recipeIndexExists;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="searchClient">the http search client</param>
    /// <param name="meterFactory">factory for making metrics</param>
    /// <param name="logger">logger for this strategy</param>
    public SearchIndexCreationStrategy(
        IMeilisearchClient searchClient,
        IMeterFactory meterFactory,
        ILogger<SearchIndexCreationStrategy> logger
    )
    {
        _logger = logger;
        _searchClient = searchClient;
        Meter meter = meterFactory.Create(
            "Reciplex.Server.Database.SearchExporter.SearchIndexCreationStrategy",
            "1.0.0"
        );

        _searchIndexUpsertTime = meter.CreateHistogram<double>(
            "reciplex.search_index_creation_strategy.index_creation_duration",
            unit: "time",
            description: "Total time it took to create a search index.",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [100, 500, 1000, 2000, 4000, 8000],
            }
        );
    }

    /// <summary>
    /// Creates the recipe index if it does not already exist
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the index exists</returns>
    public async Task<bool> UpsertRecipeIndexAsync(CancellationToken ct)
    {
        if (_recipeIndexExists)
        {
            return true;
        }

        if (
            await UpsertIndexAsync(
                RecipesSearchIndex,
                PrimaryKeyPropertyName,
                SearchExporterMetrics.RecipesKind,
                ct
            )
        )
        {
            _recipeIndexExists = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates an index with name <paramref name="indexName"/> and <paramref name="primaryKey"/>
    /// if it does not already exist.
    /// </summary>
    /// <param name="indexName">the name of the index</param>
    /// <param name="primaryKey">the primary key of the index</param>
    /// <param name="metricKind">the metric kind</param>
    /// <param name="ct">the async cancellation token</param>
    private async Task<bool> UpsertIndexAsync(
        string indexName,
        string primaryKey,
        string metricKind,
        CancellationToken ct
    )
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        string outcome = "exists";
        try
        {
            GetIndexResponse? index = await _searchClient.GetIndexAsync(indexName, ct);
            if (index is null)
            {
                outcome = "created";
                MeilisearchTaskResponse createResponse = await _searchClient.CreateIndexAsync(
                    indexName,
                    primaryKey,
                    ct
                );

                TaskStatusResponse? createTask = await _searchClient.WaitForTaskCompletionAsync(
                    createResponse.TaskUid,
                    ct
                );

                if (createTask?.Status != MeilisearchTaskStatus.Succeeded)
                {
                    outcome = "error";
                    _logger.Error_IndexCreationTaskFailed(
                        indexName,
                        createResponse.TaskUid,
                        createTask?.Status
                    );
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            outcome = SearchExporterMetrics.ErrorIndexOperationOutcomeKind;
            _logger.Error_IndexCreationFailedWithException(indexName, primaryKey, ex);
            return false;
        }
        finally
        {
            _searchIndexUpsertTime.Record(
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                new KeyValuePair<string, object?>("rows", metricKind),
                new KeyValuePair<string, object?>("outcome", outcome)
            );
        }
    }
}
