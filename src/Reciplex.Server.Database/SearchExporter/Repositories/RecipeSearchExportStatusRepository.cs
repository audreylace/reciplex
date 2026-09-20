using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.Database.SearchExporter.Repositories;

class RecipeSearchExportStatusRepository(
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
                    && (
                        e.LeaseExpireTime < before
                        || e.LeaseExpireTime > after
                        || e.LeaseExpireTime == null
                    )
                ) || (e.LeaseToken == null && e.LeaseExpireTime != null)
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
                ids.Contains(e.RecipeFk) && e.LeaseToken == null && e.LeaseExpireTime == null
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

    public async Task<List<long>> CreateSearchStatusRowsAsync(
        int max,
        string leaseToken,
        TimeSpan leaseExpireTime,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        var entries = await db
            .Recipes.AsNoTracking()
            .DeleteFieldNull()
            .Where(r =>
                r.RecipeBook!.Deleted == null
                && r.RecipeBook!.Owner!.Deleted == null
                && !db.RecipeSearchExtractionStatusEntries.Any(s => s.RecipeFk == r.Id)
            )
            .Select(r => r.Id)
            .Take(max)
            .ToListAsync(ct);
        if (entries.Count < 1)
        {
            return [];
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long expireTime = (long)(now + leaseExpireTime.TotalSeconds);
        foreach (var id in entries)
        {
            RecipeSearchWorkerStateDbObject recipeSearchIndexEntry = new()
            {
                SearchVersion = null,
                RecipeFk = id,
                LeaseExpireTime = expireTime,
                LeaseToken = leaseToken,
            };
            db.Add(recipeSearchIndexEntry);
        }

        await db.SaveChangesAsync(ct);

        return entries;
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
                searchExtractState.LeaseExpireTime == null
                && searchExtractState.LeaseToken == null
                && (
                    (
                        searchExtractState.ExtractRetryCount < maxRetries
                        && (
                            searchExtractState.NextExtractRetryTime == null
                            || searchExtractState.NextExtractRetryTime < now
                        )
                    )
                    || searchExtractState.Recipe!.SearchVersion
                        > searchExtractState.AttemptedExtractSearchVersion
                )
                &&
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

            )
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
                        .SetProperty(e => e.SearchVersion, searchVersion)
                        .SetProperty(e => e.AttemptedExtractSearchVersion, (long?)null),
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
                && e.LeaseToken == null
                && e.DeleteRetryCounter < maxRetries
                && (e.NextDeleteRetryTime == null || e.NextDeleteRetryTime < now)
            )
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

    public async Task<int> PurgeRecipeSearchEntriesWithTooManyRetries(
        int batchSize,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        var entries = await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.LeaseExpireTime == null
                && e.DeleteRetryCounter >= maxRetries
                && e.LeaseToken == null
            )
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
                            e => e.NextDeleteRetryTime,
                            e => now + (1 << e.DeleteRetryCounter)
                        )
                        .SetProperty(e => e.DeleteRetryCounter, e => e.DeleteRetryCounter + 1),
                ct
            );
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

    public async Task<int> MarkRecipesAsExtractedAndReleaseAsync(
        List<(long RecipeId, long SearchVersion)> entries,
        string leaseToken,
        CancellationToken ct
    )
    {
        List<long> ids = [.. entries.Select(e => e.RecipeId)];
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        var parameter = Expression.Parameter(typeof(RecipeSearchWorkerStateDbObject), "row");
        var recipeFkProperty = Expression.Property(
            parameter,
            nameof(RecipeSearchWorkerStateDbObject.RecipeFk)
        );
        var searchVersionProperty = Expression.Property(
            parameter,
            nameof(RecipeSearchWorkerStateDbObject.SearchVersion)
        );
        Expression caseExpression = searchVersionProperty;

        foreach (var (RecipeId, SearchVersion) in entries)
        {
            var test = Expression.Equal(recipeFkProperty, Expression.Constant(RecipeId));
            var value = Expression.Constant(SearchVersion);

            caseExpression = Expression.Condition(test, value, caseExpression);
        }

        var searchVersionLambda = Expression.Lambda<Func<RecipeSearchWorkerStateDbObject, long?>>(
            caseExpression,
            parameter
        );

        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                ids.Contains(searchExtractState.RecipeFk)
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.ExtractRetryCount, 0)
                        .SetProperty(e => e.NextExtractRetryTime, (long?)null)
                        .SetProperty(e => e.SearchVersion, searchVersionLambda)
                        .SetProperty(e => e.AttemptedExtractSearchVersion, (long?)null),
                ct
            );
    }

    public async Task<int> MarkRecipeExtractionFailedAndReleaseAsync(
        List<(long RecipeId, long SearchVersion)> entries,
        string leaseToken,
        CancellationToken ct
    )
    {
        List<long> ids = [.. entries.Select(e => e.RecipeId)];
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        var parameter = Expression.Parameter(typeof(RecipeSearchWorkerStateDbObject), "row");
        var recipeFkProperty = Expression.Property(
            parameter,
            nameof(RecipeSearchWorkerStateDbObject.RecipeFk)
        );
        var searchVersionProperty = Expression.Property(
            parameter,
            nameof(RecipeSearchWorkerStateDbObject.AttemptedExtractSearchVersion)
        );
        Expression caseExpression = searchVersionProperty;

        foreach (var (RecipeId, SearchVersion) in entries)
        {
            var test = Expression.Equal(recipeFkProperty, Expression.Constant(RecipeId));
            var value = Expression.Constant(SearchVersion);

            caseExpression = Expression.Condition(test, value, caseExpression);
        }

        var searchVersionLambda = Expression.Lambda<Func<RecipeSearchWorkerStateDbObject, long?>>(
            caseExpression,
            parameter
        );

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
            .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                ids.Contains(searchExtractState.RecipeFk)
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
                                e.Recipe!.SearchVersion == e.AttemptedExtractSearchVersion
                                    ? e.ExtractRetryCount + 1
                                    : 1
                        )
                        .SetProperty(e => e.AttemptedExtractSearchVersion, searchVersionLambda)
                        .SetProperty(
                            e => e.NextExtractRetryTime,
                            e => now + (1 << e.ExtractRetryCount)
                        ),
                ct
            );
    }
}
