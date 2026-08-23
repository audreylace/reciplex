using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchExtractionWorker;

internal interface IDurableRecordsToExtractForSearchQueue
{
    public Task PostRecipeChangeQueueEntryAsync(
        long targetRecipeFk,
        string searchVersionTag,
        RecordChangeActionKind recordChangeActionKind,
        CancellationToken ct
    );
    public Task PostBookChangeQueueEntryAsync(
        long targetRecipeBookFk,
        string searchVersionTag,
        RecordChangeActionKind recordChangeActionKind,
        CancellationToken ct
    );

    public Task<List<long>> PullStaleEntriesAsync(int max, CancellationToken ct);
}
