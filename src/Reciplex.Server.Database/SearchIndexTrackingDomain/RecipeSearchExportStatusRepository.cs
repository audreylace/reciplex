using System.Linq.Expressions;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Implements <see cref="IRecipeSearchExportStatusRepository"/>
/// </summary>
/// <param name="dbFactory">db connection factory</param>
/// <param name="clock">clock for getting utc time</param>
sealed class RecipeSearchExportStatusRepository(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IClock clock
) : IRecipeSearchExportStatusRepository
{
    /// <summary>
    /// Bounded channel used like a conditional variable. Signals search extraction when there is work.
    /// </summary>
    private readonly Channel<long> _searchIndexSyncHasWork = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <summary>
    /// Bounded channel used like a conditional variable. Signals search extraction when there is work.
    /// </summary>
    private readonly Channel<long> _searchIndexSyncHasDeleteWork = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <inheritdoc />
    public void SignalSearchExtractionWork()
    {
        _searchIndexSyncHasWork.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForSearchExtractionWorkAsync(TimeSpan timeout, CancellationToken ct)
    {
        return _searchIndexSyncHasWork.WaitOnChannel(timeout, ct);
    }

    /// <inheritdoc />
    public void SignalSearchDeleteWork()
    {
        _searchIndexSyncHasDeleteWork.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForSearchDeleteWorkAsync(TimeSpan timeout, CancellationToken ct)
    {
        return _searchIndexSyncHasDeleteWork.WaitOnChannel(timeout, ct);
    }

    /// <inheritdoc />
    public async Task<int> BreakLeasesAsync(
        TimeSpan leaseMaxLookBack,
        TimeSpan leaseMaxLookAhead,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long before = (long)(now - leaseMaxLookBack.TotalSeconds);
        long after = (long)(now + leaseMaxLookAhead.TotalSeconds);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                (
                    e.LeaseExpireTime != null
                    && (e.LeaseExpireTime < before || e.LeaseExpireTime > after)
                )
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(l => l.LeaseExpireTime, (long?)null)
                        .SetProperty(l => l.LeaseToken, (string?)null),
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        return count;
    }

    /// <inheritdoc />
    public async Task<int> PurgeRecipeSearchEntriesWithTooManyDeleteRetries(
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && e.LeaseExpireTime == null
                && e.RetryCount >= maxRetries
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<int> PurgeMarkedForDeletionAsync(CancellationToken ct)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.ExtractionStatus == SearchExtractionStatus.PendingDelete
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> MarkExtractionStatusForBookAsPendingDeleteAsync(
        long bookId,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);

        // populate search extraction entries to start search cleanup
        int count = await dbContext
            .RecipeSearchExtractionStatusEntries.Where(r =>
                r.RecipeBookFk == bookId
                && r.ExtractionStatus != SearchExtractionStatus.PendingDelete
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(r => r.RetryCount, 0)
                        .SetProperty(r => r.ExtractionStatus, SearchExtractionStatus.PendingDelete)
                        .SetProperty(r => r.NextRetryTime, (long?)null)
                        .SetProperty(r => r.LeaseToken, (string?)null)
                        .SetProperty(r => r.LeaseExpireTime, (long?)null),
                ct
            );

        if (count > 0)
        {
            SignalSearchDeleteWork();
        }

        return count;
    }

    /// <inheritdoc />
    public async Task<int> CrossSyncRecipesDeletionStatusAsync(CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        int count = await dbContext
            .RecipeSearchExtractionStatusEntries.Where(e =>
                e.Recipe!.IsDeleted && e.ExtractionStatus != SearchExtractionStatus.PendingDelete
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.RetryCount, 0)
                        .SetProperty(e => e.ExtractionStatus, SearchExtractionStatus.PendingDelete)
                        .SetProperty(e => e.NextRetryTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.LeaseExpireTime, (long?)null),
                ct
            );

        if (count > 0)
        {
            SignalSearchDeleteWork();
        }

        return count;
    }
}
