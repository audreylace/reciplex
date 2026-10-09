using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.SearchExporter;

namespace Reciplex.Server.Database.RecipeBooksDomain;

class RecipeBookDeletionWorkerHostedService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IClock clock,
    IRecipeMutationNotifyService notificationService,
    ILogger<RecipeBookDeletionWorkerHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Queue<(long Id, long When)> skipIdsQueue = [];
        HashSet<long> skipIdsHash = [];
        int lookupQueryFails = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = clock.GetCurrentInstant().ToUnixTimeSeconds();
                RemoveExpiredEntries(skipIdsQueue, skipIdsHash, now);
                using var dbContext = await dbFactory.CreateDbContextAsync(stoppingToken);
                RecipeBookDbObject? book = await dbContext
                    .RecipeBooks.Where(book =>
                        book.Deleted != null
                        && (book.NextDeletePoll == null || book.NextDeletePoll < now)
                        && skipIdsHash.Contains(book.Id)
                    )
                    .OrderBy(r => r.NextDeletePoll)
                    .ThenBy(r => r.Id)
                    .FirstOrDefaultAsync(stoppingToken);

                lookupQueryFails = 0;
                if (book is not null)
                {
                    await TryDeleteBookAsync(
                        skipIdsQueue,
                        skipIdsHash,
                        now,
                        dbContext,
                        book,
                        stoppingToken
                    );
                }
                else
                {
                    await notificationService.WaitForBookDeleteTriggerAsync(
                        TimeSpan.FromMinutes(1),
                        stoppingToken
                    );
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // delay to prevent a tight loop on repeated failure
                logger.Error_BackgroundLoopFailure(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
                lookupQueryFails++;
                if (lookupQueryFails > 10)
                {
                    skipIdsHash.Clear();
                    skipIdsQueue.Clear();
                    lookupQueryFails = 10;
                }
            }
        }
    }

    private static void RemoveExpiredEntries(
        Queue<(long Id, long When)> skipIdsQueue,
        HashSet<long> skipIdsHash,
        long now
    )
    {
        while (
            skipIdsQueue.TryPeek(out var queuePeek)
            && queuePeek.When < now
            && skipIdsQueue.TryDequeue(out var queueEntry)
        )
        {
            skipIdsHash.Remove(queueEntry.Id);
        }
    }

    private async Task TryDeleteBookAsync(
        Queue<(long Id, long When)> skipIdsQueue,
        HashSet<long> skipIdsHash,
        long now,
        ApplicationDbContext dbContext,
        RecipeBookDbObject book,
        CancellationToken stoppingToken
    )
    {
        try
        {
            bool notify = await RecipeBookDeletionHelper.CleanupBookLinksAsync(
                dbContext,
                book.Id,
                now,
                stoppingToken
            );

            if (
                !await dbContext
                    .Recipes.Where(r => r.RecipeBookFk == book.Id)
                    .AnyAsync(stoppingToken)
            )
            {
                dbContext.Remove(book);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            else
            {
                book.NextDeletePollCounter = Math.Min(16, book.NextDeletePollCounter + 1);
                book.NextDeletePoll = now + (1 << (int)book.NextDeletePollCounter);
                await dbContext.SaveChangesAsync(stoppingToken);

                if (notify)
                {
                    notificationService.TriggerSearchIndexDelete();
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error_BookDeleteFailed(book.Id, ex);
            AddIdToSkipQueue(skipIdsQueue, skipIdsHash, book.Id);
        }
    }

    private void AddIdToSkipQueue(
        Queue<(long Id, long When)> skipIdsQueue,
        HashSet<long> skipIdsHash,
        long bookId
    )
    {
        const int MaxEntries = 100;
        if (skipIdsHash.Add(bookId))
        {
            skipIdsQueue.Enqueue(
                (
                    bookId,
                    clock.GetCurrentInstant().ToUnixTimeSeconds()
                        + (long)TimeSpan.FromMinutes(1).TotalSeconds
                )
            );
            if (skipIdsQueue.Count > MaxEntries)
            {
                if (skipIdsQueue.TryDequeue(out (long Id, long When) tuple))
                {
                    skipIdsHash.Remove(tuple.Id);
                }
            }
        }
    }
}
