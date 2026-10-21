using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;

namespace Reciplex.Server.SearchExporter.Repositories;

/// <summary>
/// Implements <see cref="ISearchIndexRepository"/>
/// </summary>
/// <param name="searchClient">client for calling the remote</param>
/// <param name="logger">service logger</param>
/// <param name="options">application options</param>
sealed class MeilisearchIndexRepository(
    IMeilisearchClient searchClient,
    ILogger<MeilisearchIndexRepository> logger,
    IOptions<SearchExporterOptions> options
) : ISearchIndexRepository
{
    #region Constants

    /// <summary>
    /// Recipe search index name
    /// </summary>
    private const string RecipesSearchIndexUid = "recipeBooks";

    /// <summary>
    /// The primary key for the recipe search index
    /// </summary>
    private const string SearchIndexPrimaryKeyPropertyName = "id";

    #endregion Constants

    #region ISearchIndexRepository Implementation

    /// <inheritdoc />
    public async Task<IndexMutationOperationOutcome> DeleteDocumentsAsync(
        IEnumerable<long> rowIds,
        CancellationToken ct
    )
    {
        HashSet<long> idSet = [.. rowIds];

        if (idSet.Count < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowIds),
                "expect non-empty set of recipe ids"
            );
        }

        try
        {
            MeilisearchTaskResponse deleteTask = await searchClient.DeleteDocumentsAsync(
                RecipesSearchIndexUid,
                rowIds,
                ct
            );

            TaskStatusResponse? taskStatus = await searchClient.WaitForTaskCompletionAsync(
                deleteTask.TaskUid,
                ct
            );

            if (taskStatus is null)
            {
                logger.Error_RecipeDeleteBatchFailed(deleteTask.TaskUid);
                return IndexMutationOperationOutcome.Error;
            }
            if (taskStatus.Status != MeilisearchTaskStatus.Succeeded)
            {
                logger.Error_RecipeDeleteBatchFailed(deleteTask.TaskUid, taskStatus.Status);
                return IndexMutationOperationOutcome.BatchFailed;
            }

            return IndexMutationOperationOutcome.Success;
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_RecipeDeleteBatchFailed(ex);
            return IndexMutationOperationOutcome.Error;
        }
    }

    /// <inheritdoc />
    public async Task<bool> EnsureSetupCompleteAsync(CancellationToken ct)
    {
        if (!await CreateRecipeIndexIfNeededAsync(ct))
        {
            return false;
        }

        return await UpdateIndexFilterAttributesIfNeededAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IndexMutationOperationOutcome> UpsertDocumentsAsync(
        UpsertDocumentsInSearchIndexArgs args,
        CancellationToken ct
    )
    {
        if (args.Documents.Count < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(args),
                $"must supply at lease one document in {nameof(UpsertDocumentsInSearchIndexArgs.Documents)}"
            );
        }

        try
        {
            MeilisearchTaskResponse upsertResponse = await searchClient.UpsertDocumentsAsync(
                RecipesSearchIndexUid,
                args.Documents.Select(e =>
                    e switch
                    {
                        RecipeSearchDao r => RecipeBookSearchIndexDocument.ForRecipe(
                            r.RowId,
                            r.RecipeId,
                            r.RecipeBookId,
                            r.Name,
                            r.ShortDescription
                        ),
                        RecipeBookSearchDao b => RecipeBookSearchIndexDocument.ForBook(
                            b.RowId,
                            b.RecipeBookId,
                            b.Name,
                            b.ShortDescription
                        ),
                        _ => throw new NotImplementedException(),
                    }
                ),
                ct
            );

            TaskStatusResponse? createTask = await searchClient.WaitForTaskCompletionAsync(
                upsertResponse.TaskUid,
                ct
            );

            if (createTask is null)
            {
                logger.Error_RecipeUpsertBatchFailed(upsertResponse.TaskUid);
                return IndexMutationOperationOutcome.Error;
            }
            if (createTask.Status != MeilisearchTaskStatus.Succeeded)
            {
                logger.Error_RecipeUpsertBatchFailed(upsertResponse.TaskUid, createTask.Status);
                return IndexMutationOperationOutcome.BatchFailed;
            }

            return IndexMutationOperationOutcome.Success;
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_RecipeUpsertBatchFailed(ex);

            return IndexMutationOperationOutcome.Error;
        }
    }

    /// <inheritdoc />
    public bool IsEnabled() => options.Value.Enable;

    /// <inheritdoc />
    public async Task<List<RecipeBookSearchIndexDocument>> SearchDocumentsAsync(
        SearchDocumentIndexArgs args,
        CancellationToken ct
    )
    {
        StringBuilder filterString = new();

        // builds filter that looks like this:
        //   with books
        // kind = 2 AND ( recipeBookId = 1 OR recipeBookId = 2 )
        //   without books
        // kind = 2
        filterString.Append("kind = ");
        filterString.Append((int)RecipeBookSearchIndexDocumentKind.Recipe);
        if (args.BookIds?.Count > 0)
        {
            bool first = true;

            filterString.Append(" AND ( ");
            foreach (long id in args.BookIds)
            {
                if (!first)
                {
                    filterString.Append(" OR ");
                }

                first = false;
                filterString.Append("recipeBookId = ");
                filterString.Append(id);
            }
            filterString.Append(") ");
        }

        SearchQueryResponse<RecipeBookSearchIndexDocument>? result =
            await searchClient.SearchByPostAsync<RecipeBookSearchIndexDocument>(
                RecipesSearchIndexUid,
                new() { SearchString = args.SearchString, FilterString = filterString.ToString() },
                ct
            );

        if (result is null)
        {
            return [];
        }

        return result.Hits;
    }

    #endregion ISearchIndexRepository Implementation

    #region  Private Methods
    private async Task<bool> CreateRecipeIndexIfNeededAsync(CancellationToken ct)
    {
        try
        {
            GetIndexResponse? index = await searchClient.GetIndexAsync(RecipesSearchIndexUid, ct);
            if (index is null)
            {
                MeilisearchTaskResponse createResponse = await searchClient.CreateIndexAsync(
                    RecipesSearchIndexUid,
                    SearchIndexPrimaryKeyPropertyName,
                    ct
                );

                TaskStatusResponse? createTask = await searchClient.WaitForTaskCompletionAsync(
                    createResponse.TaskUid,
                    ct
                );

                if (createTask is null)
                {
                    logger.Error_IndexCreationFailed(
                        createResponse.TaskUid,
                        RecipesSearchIndexUid,
                        SearchIndexPrimaryKeyPropertyName
                    );
                    return false;
                }
                if (createTask.Status != MeilisearchTaskStatus.Succeeded)
                {
                    logger.Error_IndexCreationFailed(
                        createResponse.TaskUid,
                        createTask.Status,
                        RecipesSearchIndexUid,
                        SearchIndexPrimaryKeyPropertyName
                    );
                    return false;
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_IndexCreationFailed(
                RecipesSearchIndexUid,
                SearchIndexPrimaryKeyPropertyName,
                ex
            );
            return false;
        }

        return true;
    }

    private async Task<bool> UpdateIndexFilterAttributesIfNeededAsync(CancellationToken ct)
    {
        try
        {
            MeiliFilterAttributes? filterAttributes =
                await searchClient.GetFilterableAttributesAsync(RecipesSearchIndexUid, ct);

            if (filterAttributes is null)
            {
                return false;
            }

            const string RecipeBookIdPropertyKey = "recipeBookId";
            const string RecipeDocumentKind = "kind";
            if (
                filterAttributes.Properties.IndexOf(RecipeBookIdPropertyKey) == -1
                || filterAttributes.Properties.IndexOf(RecipeDocumentKind) == -1
            )
            {
                var replaceTask = await searchClient.ReplaceFilterableAttributesAsync(
                    RecipesSearchIndexUid,
                    [RecipeBookIdPropertyKey, RecipeDocumentKind],
                    ct
                );

                TaskStatusResponse? taskOutcome = await searchClient.WaitForTaskCompletionAsync(
                    replaceTask.TaskUid,
                    ct
                );

                if (taskOutcome is null)
                {
                    logger.Error_RecipeFilterAttributesUpdateFailed(replaceTask.TaskUid);
                    return false;
                }
                if (taskOutcome.Status != MeilisearchTaskStatus.Succeeded)
                {
                    logger.Error_RecipeFilterAttributesUpdateFailed(
                        replaceTask.TaskUid,
                        taskOutcome.Status
                    );
                    return false;
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_RecipeFilterAttributesUpdateFailed(ex);
            return false;
        }

        return true;
    }

    #endregion  Private Methods
}
