using System.Globalization;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

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
    private const string RecipesSearchIndexUid = "recipes";

    /// <summary>
    /// The primary key for the recipe search index
    /// </summary>
    private const string RecipeSearchIndexPrimaryKeyPropertyName = "recipeId";

    #endregion Constants

    #region ISearchIndexRepository Implementation

    /// <inheritdoc />
    public async Task<IndexMutationOperationOutcome> DeleteRecipesAsync(
        IEnumerable<long> recipeIds,
        CancellationToken ct
    )
    {
        HashSet<long> idSet = [.. recipeIds];

        if (idSet.Count < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recipeIds),
                "expect non-empty set of recipe ids"
            );
        }

        try
        {
            MeilisearchTaskResponse deleteTask = await searchClient.DeleteDocumentsAsync(
                RecipesSearchIndexUid,
                idSet.Select(id => RecipeBookSearchIndexDocument.AsRecipeId(id)),
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
    public async Task<bool> EnsureRecipeIndexSetupCompleteAsync(CancellationToken ct)
    {
        if (!await CreateRecipeIndexIfNeededAsync(ct))
        {
            return false;
        }

        return await UpdateRecipeIndexFilterAttributesIfNeededAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IndexMutationOperationOutcome> UpsertRecipesAsync(
        UpsertRecipesInSearchIndexArgs args,
        CancellationToken ct
    )
    {
        if (args.Recipes.Count < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(args),
                $"must supply at lease one recipe in {nameof(UpsertRecipesInSearchIndexArgs.Recipes)}"
            );
        }

        try
        {
            MeilisearchTaskResponse upsertResponse = await searchClient.UpsertDocumentsAsync(
                RecipesSearchIndexUid,
                args.Recipes.Select(e =>
                    RecipeBookSearchIndexDocument.ForRecipe(
                        e.RecipeId,
                        e.RecipeBookId,
                        e.Name,
                        e.ShortDescription
                    )
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
    public async Task<List<RecipeBookSearchIndexDocument>> SearchRecipesAsync(
        SearchRecipesIndexArgs args,
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
                    RecipeSearchIndexPrimaryKeyPropertyName,
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
                        RecipeSearchIndexPrimaryKeyPropertyName
                    );
                    return false;
                }
                if (createTask.Status != MeilisearchTaskStatus.Succeeded)
                {
                    logger.Error_IndexCreationFailed(
                        createResponse.TaskUid,
                        createTask.Status,
                        RecipesSearchIndexUid,
                        RecipeSearchIndexPrimaryKeyPropertyName
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
                RecipeSearchIndexPrimaryKeyPropertyName,
                ex
            );
            return false;
        }

        return true;
    }

    private async Task<bool> UpdateRecipeIndexFilterAttributesIfNeededAsync(CancellationToken ct)
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
