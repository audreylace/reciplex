using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions;
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
    private readonly Channel<long> _hasExtractionWorkChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <summary>
    /// Bounded channel used like a conditional variable. Signals search extraction when there is work.
    /// </summary>
    private readonly Channel<long> _hasDeletionWorkChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <inheritdoc />
    public async Task<int> BreakLeasesAsync(
        TimeSpan leaseMaxLookBack,
        TimeSpan leaseMaxLookAhead,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();
        long before = (long)(now - leaseMaxLookBack.TotalSeconds);
        long after = (long)(now + leaseMaxLookAhead.TotalSeconds);

        return await db
            .SearchExtractionTrackers.Where(e =>
                (
                    e.LeaseExpireTime != null
                    && (e.LeaseExpireTime < before || e.LeaseExpireTime > after)
                )
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> ClearLeasesAsync(
        List<long> rowIds,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .SearchExtractionTrackers.Where(e =>
                rowIds.Contains(e.Id) && e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> RenewLeasesAsync(
        List<long> rowIds,
        string leaseToken,
        TimeSpan leaseExpireTime,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();
        long expireTime = (long)(now + leaseExpireTime.TotalSeconds);
        return await db
            .SearchExtractionTrackers.Where(e =>
                rowIds.Contains(e.Id) && e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.LeaseExpireTime, expireTime), ct);
    }

    /// <inheritdoc />
    public async Task<int> ClaimLeasesAsync(
        List<long> rowIds,
        string leaseToken,
        TimeSpan leaseExpireTime,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();
        long expireTime = (long)(now + leaseExpireTime.TotalSeconds);
        return await db
            .SearchExtractionTrackers.Where(e =>
                rowIds.Contains(e.Id) && e.LeaseExpireTime == null && e.LeaseToken == null
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, expireTime)
                        .SetProperty(e => e.LeaseToken, leaseToken),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<List<long>> GetClaimedRowsIdsAsync(string leaseToken, CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .SearchExtractionTrackers.Where(e =>
                e.LeaseToken == leaseToken && e.LeaseExpireTime != null
            )
            .Select(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<IExtractedSearchRow>> ExtractRecordsAsync(
        List<long> rowIds,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .SearchExtractionTrackers.AsNoTracking()
            .Where(searchExtractState =>
                rowIds.Contains(searchExtractState.Id)
                && searchExtractState.ExtractionStatus == SearchExtractionStatus.PendingExtraction
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .Select(row =>
                (IExtractedSearchRow)(
                    row.RowKind == SearchExportRowKind.Book
                        ? new BookRecordDataExtractedFromDatabase(
                            row.Id,
                            row.Book!.Name,
                            row.Book!.ShortDescription,
                            row.RecipeBookFk
                        )
                    : row.RowKind == SearchExportRowKind.Recipe && row.RecipeFk != null
                        ? new RecipeRecordDataExtractedFromDatabase(
                            row.Id,
                            (long)row.RecipeFk!,
                            row.Recipe!.Name,
                            row.Recipe!.ShortDescription,
                            row.RecipeBookFk
                        )
                    : new InvalidRowExtractedFromDatabase(row.Id)
                )
            )
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<long>> GetPendingRecordsAsync(
        int limit,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();
        return await db
            .SearchExtractionTrackers.AsNoTracking()
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
            .OrderBy(row => row.NextRetryTime)
            .ThenBy(row => row.Id)
            .Select(row => row.Id)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> RecordExtractionSuccessAsync(
        long id,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .SearchExtractionTrackers.Where(searchExtractState =>
                searchExtractState.Id == id
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
    public async Task<int> RecordExtractionFailureAsync(
        long rowId,
        string leaseToken,
        int maxRetries,
        CancellationToken ct
    )
    {
        return await UpdateRetryCountersAsync(
            rowId,
            maxRetries,
            leaseToken,
            SearchExtractionStatus.PendingExtraction,
            ct
        );
    }

    /// <inheritdoc />
    public async Task<List<long>> GetRowsToDeleteAsync(
        int limit,
        int maxRetries,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();
        return await db
            .SearchExtractionTrackers.Where(e =>
                e.LeaseExpireTime == null
                && e.LeaseToken == null
                && e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && (e.NextRetryTime == null || e.NextRetryTime < now)
                && e.RetryCount < maxRetries
            )
            .OrderBy(row => row.NextRetryTime)
            .ThenBy(row => row.Id)
            .Select(row => row.Id)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> PurgeRowsWithMaxDeleteAttemptsAsync(int maxRetries, CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await db
            .SearchExtractionTrackers.Where(e =>
                e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && e.LeaseExpireTime == null
                && e.LeaseToken == null
                && e.RetryCount >= maxRetries
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> FinishRowsDeletionAsync(
        List<long> rowIds,
        string leaseToken,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        int count = await db
            .SearchExtractionTrackers.Where(e =>
                rowIds.Contains(e.Id)
                && e.LeaseToken == leaseToken
                && e.LeaseExpireTime != null
                && e.ExtractionStatus == SearchExtractionStatus.PendingDelete
            )
            .ExecuteDeleteAsync(ct);

        return count;
    }

    /// <inheritdoc />
    public async Task<int> RecordRowDeletionFailedAsync(
        long rowId,
        long maxRetries,
        string leaseToken,
        CancellationToken ct
    )
    {
        return await UpdateRetryCountersAsync(
            rowId,
            maxRetries,
            leaseToken,
            SearchExtractionStatus.PendingDelete,
            ct
        );
    }

    private async Task<int> UpdateRetryCountersAsync(
        long rowId,
        long maxRetries,
        string leaseToken,
        SearchExtractionStatus searchExtractionStatus,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);
        long now = clock.NowUtcUnixSeconds();

        var row = await db
            .SearchExtractionTrackers.Where(searchExtractState =>
                searchExtractState.Id == rowId
                && searchExtractState.ExtractionStatus == searchExtractionStatus
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
            )
            .Select(row => new { row.RetryCount })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return 0;
        }

        var newRetry = Math.Min(maxRetries, row.RetryCount + 1);
        var nextRetryTime = now + (1 << row.RetryCount); // start at 0 for 1 second

        return await db
            .SearchExtractionTrackers.Where(searchExtractState =>
                searchExtractState.Id == rowId
                && searchExtractState.ExtractionStatus == searchExtractionStatus
                && searchExtractState.LeaseToken == leaseToken
                && searchExtractState.LeaseExpireTime != null
                && searchExtractState.RetryCount == row.RetryCount
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, e => newRetry)
                        .SetProperty(e => e.NextRetryTime, e => nextRetryTime),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<int> PurgeAllRowsMarkedForDeletionAsync(CancellationToken ct)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .SearchExtractionTrackers.Where(e =>
                e.ExtractionStatus == SearchExtractionStatus.PendingDelete
                && e.LeaseExpireTime == null
                && e.LeaseToken == null
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> SetBookAndRecipesAsPendingDeleteAsync(long bookId, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);

        // set book status to pending
        int rowCount = await dbContext
            .SearchExtractionTrackers.Where(r =>
                r.RecipeBookFk == bookId
                && r.ExtractionStatus != SearchExtractionStatus.PendingDelete
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

        if (rowCount > 0)
        {
            SignalDeletionWork();
        }

        return rowCount > 0;
    }

    /// <inheritdoc />
    public async Task<int> CrossSyncRecipesDeletionStatusAsync(CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        int rowCount = await dbContext
            .SearchExtractionTrackers.Where(e =>
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

        if (rowCount > 0)
        {
            SignalDeletionWork();
        }

        return rowCount;
    }

    /// <inheritdoc />
    public void SignalExtractionWork()
    {
        _hasExtractionWorkChannel.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForExtractionWorkAsync(TimeSpan timeout, CancellationToken ct)
    {
        return _hasExtractionWorkChannel.WaitOnChannel(timeout, ct);
    }

    /// <inheritdoc />
    public void SignalDeletionWork()
    {
        _hasDeletionWorkChannel.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForDeletionWorkAsync(TimeSpan timeout, CancellationToken ct)
    {
        return _hasDeletionWorkChannel.WaitOnChannel(timeout, ct);
    }
}
