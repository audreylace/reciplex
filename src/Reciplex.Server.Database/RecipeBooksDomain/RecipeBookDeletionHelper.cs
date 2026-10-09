using System.Data;
using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

static class RecipeBookDeletionHelper
{
    public static async Task<bool> CleanupBookLinksAsync(
        ApplicationDbContext dbContext,
        long bookId,
        long now,
        CancellationToken ct
    )
    {
        bool any = false;

        // drop access entries right away
        await dbContext
            .RecipeBookAccessEntries.Where(access => access.RecipeBookFk == bookId)
            .ExecuteDeleteAsync(ct);

        // drop recipes that require no cleanup
        await dbContext
            .Recipes.Where(r => r.RecipeBookFk == bookId && r.RecipeSearchExtraction == null)
            .ExecuteDeleteAsync(ct);

        // flag recipes deleted so they stop showing up while background cleanup is in progress
        any =
            await dbContext
                .Recipes.Where(r => r.RecipeBookFk == bookId && r.Deleted == null)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Deleted, now), ct) > 0;

        // populate search extraction entries to start search cleanup
        return await dbContext
                .RecipeSearchExtractionStatusEntries.Where(state =>
                    state.RecipeBookFk == bookId
                    && state.ExtractionStatus != SearchExtractionStatus.PendingDelete
                )
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(e => e.ExtractionStatus, SearchExtractionStatus.PendingDelete)
                            .SetProperty(
                                e => e.ExtractionStatus,
                                SearchExtractionStatus.PendingDelete
                            )
                            .SetProperty(e => e.LeaseExpireTime, (long?)null)
                            .SetProperty(e => e.LeaseToken, (string?)null)
                            .SetProperty(e => e.RetryCount, 0)
                            .SetProperty(e => e.NextRetryTime, (long?)null),
                    ct
                ) > 0
            || any;
    }
}
