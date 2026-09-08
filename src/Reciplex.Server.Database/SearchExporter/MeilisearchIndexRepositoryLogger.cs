using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

static partial class MeilisearchIndexRepositoryLogger
{
    [LoggerMessage(LogLevel.Error, "Failed to create {Index} with {PrimaryKey}.")]
    public static partial void Error_IndexCreationFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        string Index,
        string PrimaryKey,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Task {TaskId} failed with status {Status} when creating {Index} with {PrimaryKey}."
    )]
    public static partial void Error_IndexCreationFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId,
        MeilisearchTaskStatus Status,
        string Index,
        string PrimaryKey
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got null response waiting for task completion when creating {Index} with {PrimaryKey}. Task id is {TaskId}."
    )]
    public static partial void Error_IndexCreationFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId,
        string Index,
        string PrimaryKey
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got null response waiting for task completion when inserting documents into the recipe index. Task id is {TaskId}."
    )]
    public static partial void Error_RecipeUpsertBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId
    );

    [LoggerMessage(LogLevel.Error, "Recipe inset task {TaskId} failed with status {Status}")]
    public static partial void Error_RecipeUpsertBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId,
        MeilisearchTaskStatus Status
    );

    [LoggerMessage(LogLevel.Error, "Inserting documents into the recipe index failed.")]
    public static partial void Error_RecipeUpsertBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got null response waiting for task completion when deleting documents from the recipe index. Task id is {TaskId}."
    )]
    public static partial void Error_RecipeDeleteBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId
    );

    [LoggerMessage(LogLevel.Error, "Recipe delete task {TaskId} failed with status {Status}")]
    public static partial void Error_RecipeDeleteBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId,
        MeilisearchTaskStatus Status
    );

    [LoggerMessage(LogLevel.Error, "Deleting documents from the recipe index failed.")]
    public static partial void Error_RecipeDeleteBatchFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Got null response waiting for task completion when updating filter attributes for the recipe index. Task id is {TaskId}."
    )]
    public static partial void Error_RecipeFilterAttributesUpdateFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId
    );

    [LoggerMessage(
        LogLevel.Error,
        "Recipe filter attribute task {TaskId} failed with status {Status}"
    )]
    public static partial void Error_RecipeFilterAttributesUpdateFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        long TaskId,
        MeilisearchTaskStatus Status
    );

    [LoggerMessage(LogLevel.Error, "Recipe index filter attribute reconciliation failed.")]
    public static partial void Error_RecipeFilterAttributesUpdateFailed(
        this ILogger<MeilisearchIndexRepository> logger,
        Exception ex
    );
}
