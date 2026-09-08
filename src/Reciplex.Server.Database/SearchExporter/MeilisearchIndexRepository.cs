using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Implements <see cref="ISearchIndexRepository"/>
/// </summary>
/// <param name="searchClient">client for calling the remote</param>
/// <param name="logger">service logger</param>
sealed class MeilisearchIndexRepository(
    MeilisearchClient searchClient,
    ILogger<MeilisearchIndexRepository> logger
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
        HashSet<string> stringIds =
        [
            .. recipeIds.Select(id => RecordIdAsStringForSearch.MakeRecipeStringKey(id)),
        ];

        if (stringIds.Count < 1)
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
                recipeIds.Select(id => RecordIdAsStringForSearch.MakeRecipeStringKey(id)),
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
        catch (Exception ex) when (ex is not OperationCanceledException)
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
                args.Recipes.Select(e => new RecipeSearchIndexEntry()
                {
                    RecipeId = RecordIdAsStringForSearch.MakeRecipeStringKey(e.RecipeId),
                    Name = e.Name,
                    ShortDescription = e.ShortDescription,
                    RecipeBookId = RecordIdAsStringForSearch.MakeRecipeBookStringKey(
                        e.RecipeBookId
                    ),
                }),
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_RecipeUpsertBatchFailed(ex);

            return IndexMutationOperationOutcome.Error;
        }
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
        catch (Exception ex) when (ex is not OperationCanceledException)
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
            if (filterAttributes.Properties.IndexOf(RecipeBookIdPropertyKey) == -1)
            {
                var replaceTask = await searchClient.ReplaceFilterableAttributesAsync(
                    RecipesSearchIndexUid,
                    [RecipeBookIdPropertyKey],
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_RecipeFilterAttributesUpdateFailed(ex);
            return false;
        }

        return true;
    }
    #endregion  Private Methods
}
