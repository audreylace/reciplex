using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed class RecipeSearchExportStatusRepository(
    IDbContextFactory<ApplicationDbContext> dbFactory,
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
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .Select(r => new RecipeRecordDataExtractedFromDatabase(
                r.RecipeFk,
                r.Recipe!.Name,
                r.Recipe!.ShortDescription,
                r.Recipe!.RecipeBookFk,
                r.Recipe!.SearchVersion
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
                !searchExtractState.Extracted
                && searchExtractState.LeaseExpireTime == null
                && (
                    (
                        // filter out records that are broken and respect retry backoff
                        searchExtractState.ExtractRetryCount < maxRetries
                        && (
                            searchExtractState.NextExtractRetryTime == null
                            || searchExtractState.NextExtractRetryTime < now
                        )
                    )
                )
                // filter soft deleted records
                && searchExtractState.Recipe!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Owner!.Deleted == null
            )
            .OrderBy(e => e.RecipeFk)
            .Select(e => e.RecipeFk)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<int> MarkRecipeAsExtractedAndReleaseAsync(
        long id,
        long searchVersion,
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
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.ExtractRetryCount, 0)
                        .SetProperty(e => e.NextExtractRetryTime, (long?)null)
                        .SetProperty(
                            e => e.Extracted,
                            e =>
                                db.Recipes.Where(r => r.Id == e.RecipeFk)
                                    .Select(r => r.SearchVersion)
                                    .FirstOrDefault() == searchVersion
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
        long searchVersion,
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
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(
                            e => e.ExtractRetryCount,
                            e =>
                                db.Recipes.Where(r => r.Id == e.RecipeFk)
                                    .Select(r => r.SearchVersion)
                                    .FirstOrDefault() == searchVersion
                                    ? e.ExtractRetryCount + 1
                                    : 0
                        )
                        .SetProperty(
                            e => e.NextExtractRetryTime,
                            e =>
                                db.Recipes.Where(r => r.Id == e.RecipeFk)
                                    .Select(r => r.SearchVersion)
                                    .FirstOrDefault() == searchVersion
                                    ? now + (1 << e.ExtractRetryCount + 1)
                                    : null
                        )
                        .SetProperty(
                            e => e.Extracted,
                            e =>
                                db.Recipes.Where(r => r.Id == e.RecipeFk)
                                    .Select(r => r.SearchVersion)
                                    .FirstOrDefault() == searchVersion
                                    && e.ExtractRetryCount + 1 >= maxRetries
                                || e.Extracted
                        ),
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
                (
                    e.Recipe!.Deleted != null
                    || e.Recipe.RecipeBook!.Deleted != null
                    || e.Recipe.RecipeBook!.Owner!.Deleted != null
                )
                && e.LeaseExpireTime == null
                && e.DeleteRetryCounter < maxRetries
                && (e.NextDeleteRetryTime == null || e.NextDeleteRetryTime < now)
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
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .ExecuteDeleteAsync(ct);
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
                e.LeaseExpireTime == null && e.DeleteRetryCounter >= maxRetries
            )
            .OrderBy(e => e.RecipeFk)
            .Select(e => e.RecipeFk)
            .Take(batchSize)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return 0;
        }

        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                entries.Contains(e.RecipeFk)
                && e.DeleteRetryCounter >= maxRetries
                && e.LeaseExpireTime == null
            )
            .ExecuteDeleteAsync(ct);
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
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(
                            e => e.NextDeleteRetryTime,
                            e => now + (1 << e.DeleteRetryCounter)
                        )
                        .SetProperty(e => e.DeleteRetryCounter, e => e.DeleteRetryCounter + 1),
                ct
            );
    }
}
