using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

class RecipeSearchExportStatusRepository(ApplicationDbContext db, IClock clock)
    : IRecipeSearchExportStatusRepository
{
    /// <inheritdoc />
    public async Task<int> BreakLeasesAsync(
        int max,
        long leaseMaxLookBack,
        long leaseMaxLookAhead,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        var entries = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.LeaseExpireTime != null
                && (
                    e.LeaseExpireTime < (now - leaseMaxLookBack)
                    || e.LeaseExpireTime > (now + leaseMaxLookAhead)
                )
            )
            .Select(e => new
            {
                e.RecipeFk,
                e.LeaseToken,
                e.LeaseExpireTime,
            })
            .Take(max)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return 0;
        }

        int count = 0;
        using var transaction = await db.Database.BeginTransactionAsync(ct);
        foreach (var entry in entries)
        {
            count += await db
                .RecipeSearchExtractionStatusEntries.Where(e =>
                    e.RecipeFk == entry.RecipeFk
                    && e.LeaseToken == entry.LeaseToken
                    && e.LeaseExpireTime == e.LeaseExpireTime
                )
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                            .SetProperty(e => e.LeaseToken, (string?)null),
                    ct
                );
        }
        await transaction.CommitAsync(ct);

        return count;
    }

    /// <inheritdoc />
    public async Task<int> ClaimAsync(
        List<long> ids,
        string leaseToken,
        long leaseExpireTime,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, now + leaseExpireTime)
                        .SetProperty(e => e.LeaseToken, leaseToken),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> ClearLeasesAsync(List<long> ids, string leaseToken, CancellationToken ct)
    {
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == leaseToken
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null),
                ct
            );
    }

    public async Task<int> CreateSearchStatusRowsAsync(int max, CancellationToken ct)
    {
        var entries = await db
            .Recipes.AsNoTracking()
            .DeleteFieldNull()
            .Where(r =>
                r.RecipeSearchExtraction == null
                && r.RecipeBook!.Deleted == null
                && r.RecipeBook!.Owner!.Deleted == null
            )
            .Select(r => r.Id)
            .Take(max)
            .ToListAsync(ct);
        if (entries.Count < 1)
        {
            return 0;
        }

        foreach (var id in entries)
        {
            RecipeSearchWorkerStateDbObject recipeSearchIndexEntry = new()
            {
                SearchVersion = null,
                RecipeFk = id,
            };
            db.Add(recipeSearchIndexEntry);
        }

        return await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<List<RecipeRecordDataExtractedFromDatabase>> ExtractRecipeDataAsync(
        List<long> ids,
        string leaseToken,
        CancellationToken ct
    )
    {
        return db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                ids.Contains(searchExtractState.RecipeFk)
                && searchExtractState.LeaseToken == leaseToken
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
        int max,
        int maxRetries,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                // look for records never extracted or have since changed
                (
                    searchExtractState.SearchVersion == null
                    || searchExtractState.Recipe!.SearchVersion > searchExtractState.SearchVersion
                )
                // filter soft deleted records
                && searchExtractState.Recipe!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Owner!.Deleted == null
                // filter out records that are broken and respect retry backoff
                && (
                    (
                        searchExtractState.ExtractRetryCount < maxRetries
                        && (
                            searchExtractState.NextExtractRetryTime == null
                            || searchExtractState.NextExtractRetryTime < now
                        )
                    )
                    || searchExtractState.Recipe.SearchVersion
                        > searchExtractState.AttemptedExtractSearchVersion
                )
                && searchExtractState.LeaseExpireTime == null
                && searchExtractState.LeaseToken == null
            )
            .Select(e => e.RecipeFk)
            .Take(max)
            .ToListAsync(ct);
    }

    public async Task<int> MarkRecipeAsExtractedAndReleaseAsync(
        long id,
        long searchVersion,
        string leaseToken,
        CancellationToken ct
    )
    {
        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                searchExtractState.RecipeFk == id && searchExtractState.LeaseToken == leaseToken
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.ExtractRetryCount, 0)
                        .SetProperty(e => e.NextExtractRetryTime, (long?)null)
                        .SetProperty(e => e.SearchVersion, searchVersion)
                        .SetProperty(e => e.AttemptedExtractSearchVersion, (long?)null),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> RenewLeasesAsync(
        List<long> ids,
        string leaseToken,
        long leaseExpireTime,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                ids.Contains(e.RecipeFk) && e.LeaseToken == leaseToken
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.LeaseExpireTime, now + leaseExpireTime),
                ct
            );
    }

    public async Task<int> MarkRecipeExtractionFailedAndReleaseAsync(
        long id,
        long searchVersion,
        string leaseToken,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                searchExtractState.RecipeFk == id && searchExtractState.LeaseToken == leaseToken
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(
                            e => e.ExtractRetryCount,
                            e =>
                                e.Recipe!.SearchVersion == e.AttemptedExtractSearchVersion
                                    ? e.ExtractRetryCount + 1
                                    : 1
                        )
                        .SetProperty(e => e.AttemptedExtractSearchVersion, searchVersion)
                        .SetProperty(
                            e => e.NextExtractRetryTime,
                            e => now + (1 << e.ExtractRetryCount)
                        ),
                ct
            );
    }

    public async Task<List<long>> GetRecipesToDeleteAsync(
        int max,
        int maxRetries,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                (
                    e.Recipe!.Deleted != null
                    || e.Recipe.RecipeBook!.Deleted != null
                    || e.Recipe.RecipeBook!.Owner!.Deleted != null
                )
                && e.LeaseExpireTime == null
                && e.LeaseToken == null
                && e.DeleteRetryCounter < maxRetries
                && (e.NextDeleteRetryTime == null || e.NextDeleteRetryTime < now)
            )
            .Select(e => e.RecipeFk)
            .ToListAsync(ct);
    }

    public Task<int> DeleteRecipeSearchEntry(int recipeId, string leaseToken, CancellationToken ct)
    {
        return db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.RecipeFk == recipeId && e.LeaseToken == leaseToken
            )
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> PurgeRecipeSearchEntriesWithTooManyRetries(
        int max,
        int maxRetries,
        CancellationToken ct
    )
    {
        var entries = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.LeaseExpireTime == null
                && e.DeleteRetryCounter >= maxRetries
                && e.LeaseToken == null
            )
            .Select(e => e.RecipeFk)
            .Take(max)
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
                && e.LeaseToken == null
            )
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> MarkRecipeDeletionFailedAndReleaseAsync(
        long id,
        string leaseToken,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();

        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                searchExtractState.RecipeFk == id && searchExtractState.LeaseToken == leaseToken
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
