using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed class RecipeSearchExportStatusRepository(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IRecipeMutationNotifyService mutationNotifyService,
    IClock clock
) : IRecipeSearchExportStatusRepository
{
    /// <inheritdoc />
    public async Task<int> BreakLeasesAsync(
        int max,
        TimeSpan leaseMaxLookBack,
        TimeSpan leaseMaxLookAhead,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long before = (long)(now - leaseMaxLookBack.TotalSeconds);
        long after = (long)(now + leaseMaxLookAhead.TotalSeconds);
        var entries = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                (
                    e.LeaseExpireTime != null
                    && (e.LeaseExpireTime < before || e.LeaseExpireTime > after)
                )
            )
            .Select(e => new
            {
                e.RecipeFk,
                e.LeaseToken,
                e.LeaseExpireTime,
            })
            .OrderBy(e => e.RecipeFk)
            .Take(max)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return 0;
        }

        var parameter = Expression.Parameter(typeof(RecipeSearchWorkerStateDbObject), "e");
        Expression? combinedPredicate = null;

        foreach (var c in entries)
        {
            // Build: e.RecipeFk == c.RecipeFk && e.LeaseToken == c.LeaseToken && e.LeaseExpireTime == c.LeaseExpireTime
            var fkMatch = Expression.Equal(
                Expression.Property(parameter, nameof(RecipeSearchWorkerStateDbObject.RecipeFk)),
                Expression.Constant(c.RecipeFk)
            );

            var tokenMatch = Expression.Equal(
                Expression.Property(parameter, nameof(RecipeSearchWorkerStateDbObject.LeaseToken)),
                Expression.Constant(c.LeaseToken, typeof(string))
            );

            var expireMatch = Expression.Equal(
                Expression.Property(
                    parameter,
                    nameof(RecipeSearchWorkerStateDbObject.LeaseExpireTime)
                ),
                Expression.Constant(c.LeaseExpireTime, typeof(long?))
            );

            var rowMatch = Expression.AndAlso(fkMatch, Expression.AndAlso(tokenMatch, expireMatch));

            combinedPredicate =
                combinedPredicate == null
                    ? rowMatch
                    : Expression.OrElse(combinedPredicate, rowMatch);
        }

        if (combinedPredicate is null)
        {
            return 0;
        }

        var lambda = Expression.Lambda<Func<RecipeSearchWorkerStateDbObject, bool>>(
            combinedPredicate,
            parameter
        );

        return await db
            .RecipeSearchExtractionStatusEntries.Where(lambda)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> ClaimAsync(
        List<long> ids,
        string leaseToken,
        TimeSpan leaseExpireTime,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long expireTime = (long)(now + leaseExpireTime.TotalSeconds);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseExpireTime == null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, expireTime)
                        .SetProperty(e => e.LeaseToken, leaseToken),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> ClearLeasesAsync(List<long> ids, string leaseToken, CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<List<RecipeRecordDataExtractedFromDatabase>> ExtractRecipeDataAsync(
        List<long> ids,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                ids.Contains(searchExtractState.RecipeFk)
                && searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingExtraction
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .Select(r => new RecipeRecordDataExtractedFromDatabase(
                r.RecipeFk,
                r.Recipe!.Name,
                r.Recipe!.ShortDescription,
                r.Recipe!.RecipeBookFk
            ))
            .ToListAsync(ct);
    }

    public async Task<List<long>> GetRecipesToExtractAsync(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                // look for records never extracted or have since changed
                searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingExtraction
                && searchExtractState.LeaseExpireTime == null
                && (
                    // filter out records that are broken and respect retry backoff
                    (
                        searchExtractState.NextRetryTime == null
                        || searchExtractState.NextRetryTime < now
                    )
                    && searchExtractState.RetryCount < maxRetries
                )
            )
            .OrderBy(e => e.RecipeFk)
            .Select(e => e.RecipeFk)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<int> MarkRecipeAsExtractedAndReleaseAsync(
        long id,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                searchExtractState.RecipeFk == id
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
                && searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingExtraction
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, 0)
                        .SetProperty(e => e.NextRetryTime, (long?)null)
                        .SetProperty(
                            e => e.ExtractionStatus,
                            e => SearchExtractionStatus.Extracted
                        ),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> RenewLeasesAsync(
        List<long> ids,
        string leaseToken,
        TimeSpan leaseExpireTime,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long expireTime = (long)(now + leaseExpireTime.TotalSeconds);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.LeaseExpireTime, expireTime), ct);
    }

    public async Task<int> MarkRecipeExtractionFailedAndReleaseAsync(
        long id,
        string leaseToken,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                searchExtractState.RecipeFk == id
                && searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingExtraction
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, e => e.RetryCount + 1)
                        .SetProperty(e => e.NextRetryTime, e => now + (1 << e.RetryCount + 1)),
                ct
            );
    }

    public async Task<List<long>> GetRecipesToDeleteAsync(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.LeaseExpireTime == null
                && e.LeaseToken == null
                && e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && (e.NextRetryTime == null || e.NextRetryTime < now)
                && e.RetryCount < maxRetries
            )
            .OrderBy(e => e.RecipeFk)
            .Select(e => e.RecipeFk)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteRecipeSearchEntries(
        List<long> ids,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        int count = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk)
                && e.LeaseToken == leaseToken
                && e.LeaseExpireTime != null
                && e.ExtractionStatus == SearchExtractionStatus.PendingDelete
            )
            .ExecuteDeleteAsync(ct);

        if (count > 0)
        {
            mutationNotifyService.TriggerRecipeDelete();
        }

        return count;
    }

    public async Task<int> PurgeRecipeSearchEntriesWithTooManyDeleteRetries(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        var entries = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && e.LeaseExpireTime == null
                && e.RetryCount >= maxRetries
            )
            .OrderBy(e => e.RecipeFk)
            .Select(e => e.RecipeFk)
            .Take(batchSize)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return 0;
        }

        int count = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                entries.Contains(e.RecipeFk)
                && e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && e.RetryCount >= maxRetries
                && e.LeaseExpireTime == null
            )
            .ExecuteDeleteAsync(ct);

        if (count > 0)
        {
            mutationNotifyService.TriggerRecipeDelete();
        }

        return count;
    }

    public async Task<List<long>> GetClaimedRecipes(string leaseToken, CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .Select(e => e.RecipeFk)
            .ToListAsync(ct);
    }

    public async Task<int> MarkRecipesDeletionFailedAndReleaseAsync(
        List<long> recipeIds,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();

        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                recipeIds.Contains(searchExtractState.RecipeFk)
                && searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, e => now + (1 << e.RetryCount))
                        .SetProperty(e => e.RetryCount, e => e.RetryCount + 1),
                ct
            );
    }
}
