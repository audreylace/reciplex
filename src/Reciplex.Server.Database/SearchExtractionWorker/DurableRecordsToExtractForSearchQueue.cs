using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchExtractionWorker;

/// <summary>
/// Bridge for wiring durable queue entries into the database. This is registered as scoped since its imported
/// into generator classes.
/// </summary>
/// <param name="searchExtractionOptions">search options</param>
/// <param name="clock">the clock</param>
/// <param name="dbContext">application db context</param>
/// <param name="broadcaster">broadcasts entries to interested parties</param>
internal class DurableRecordsToExtractForSearchQueue(
    IOptions<SearchExtractionOptions> searchExtractionOptions,
    IClock clock,
    ApplicationDbContext dbContext,
    IDurableChangeQueueEntryPostedBroadcaster broadcaster
) : IDurableRecordsToExtractForSearchQueue
{
    public async Task PostRecipeChangeQueueEntryAsync(
        long targetRecipeFk,
        string searchVersionTag,
        RecordChangeActionKind recordChangeActionKind,
        CancellationToken ct
    )
    {
        if (searchExtractionOptions.Value.Enable)
        {
            long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
            RecordDbObjectChangeEntry changeEntry = new()
            {
                RecordKind = RecordChangeSourceKind.Recipe,
                ChangeKind = recordChangeActionKind,
                Created = now,
                ObservedSearchVersionTag = searchVersionTag,
                TargetRecipeFk = targetRecipeFk,
                Pulled = now,
            };
            dbContext.Add(changeEntry);
            await dbContext.SaveChangesAsync(ct);
            broadcaster.BroadcastEntryAdded(changeEntry.Id);
        }
    }

    public async Task PostBookChangeQueueEntryAsync(
        long targetRecipeBookFk,
        string searchVersionTag,
        RecordChangeActionKind recordChangeActionKind,
        CancellationToken ct
    )
    {
        if (searchExtractionOptions.Value.Enable)
        {
            long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
            RecordDbObjectChangeEntry changeEntry = new()
            {
                RecordKind = RecordChangeSourceKind.Book,
                ChangeKind = recordChangeActionKind,
                Created = now,
                ObservedSearchVersionTag = searchVersionTag,
                TargetRecipeBookFk = targetRecipeBookFk,
                Pulled = now,
            };
            dbContext.Add(changeEntry);
            await dbContext.SaveChangesAsync(ct);
            broadcaster.BroadcastEntryAdded(changeEntry.Id);
        }
    }

    public async Task<List<long>> PullStaleEntriesAsync(int max, CancellationToken ct)
    {
        long stale = clock.GetCurrentInstant().ToUnixTimeSeconds() - 10 * 60;
        List<long> ids = await dbContext
            .RecordChangeQueue.Where(e => e.Pulled < stale)
            .Select(e => e.Id)
            .Take(max)
            .ToListAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        dbContext
            .RecordChangeQueue.Where(r => ids.Contains(r.Id))
            .ExecuteUpdate(setters => setters.SetProperty(r => r.Pulled, now));

        return ids;
    }
}
