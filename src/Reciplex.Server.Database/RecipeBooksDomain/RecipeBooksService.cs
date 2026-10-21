using System.Data;
using System.Security.Cryptography;
using System.Threading.Channels;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Implements <see cref="IRecipeBookService"/>
/// </summary>
/// <param name="clock">application clock</param>
/// <param name="concurrencyTagProvider">service for generating concurrency tags</param>
/// <param name="dbFactory">db connection factory</param>
/// <param name="recipeSearchExportStatusRepository">repository for getting search status entries</param>
internal sealed class RecipeBooksService(
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository
) : IRecipeBooksService
{
    /// <summary>
    /// Bounded channel used like a conditional variable
    /// </summary>
    private readonly Channel<long> _bookDeletionSignal = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <inheritdoc />
    public void SignalDeletion()
    {
        _bookDeletionSignal.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForDeleteSignal(TimeSpan timeout, CancellationToken ct)
    {
        return _bookDeletionSignal.WaitOnChannel(timeout, ct);
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeBookAsync(long userId, CreateRecipeBookArgs createArgs, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        CreateRecipeBookArgsValidator validation = new();
        ValidationResult validationResult = await validation.ValidateAsync(createArgs, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        long now = clock.NowUtcUnixSeconds();
        RecipeBookDbObject bookDbObject = new()
        {
            Name = createArgs.Name,
            ShortDescription = createArgs.ShortDescription,
            LastModified = now,
            Created = now,
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
            OwnerFk = userId,
        };
        dbContext.Add(bookDbObject);
        var searchStatus = new SearchExtractionStatusDbObject
        {
            Book = bookDbObject,
            RowKind = SearchExportRowKind.Book,
        };
        dbContext.Add(searchStatus);
        await dbContext.SaveChangesAsync(ct);
        recipeSearchExportStatusRepository.SignalExtractionWork();
        return new SuccessResult<RecipeBookDao>(
            ToRecipeBookDao(bookDbObject, BookPermissionFlags.OwnerPermissions())
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            ConflictResult
        >
    > QueueRecipeBookDeleteAsync(long bookId, long userId, string ocTag, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct, withTracking: true);
        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayDeleteBook)
        {
            return new ForbiddenResult();
        }

        RecipeBookDbObject book = bookQuery.Book;
        if (book.ConcurrencyTag != ocTag)
        {
            return new ConflictResult();
        }

        long now = clock.NowUtcUnixSeconds();
        MarkBookDirty(book, now);
        book.IsDeleted = true;
        book.DeletedTime = now;

        try
        {
            using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(ct);
            await ResetExtractionStatusAsync(
                bookId,
                dbContext,
                SearchExtractionStatus.PendingDelete,
                ct
            );
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictResult();
        }

        SignalDeletion();

        return new EmptySuccessResult();
    }

    private static async Task ResetExtractionStatusAsync(
        long bookId,
        ApplicationDbContext dbContext,
        SearchExtractionStatus status,
        CancellationToken ct
    )
    {
        await dbContext
            .SearchExtractionTrackers.Where(row =>
                row.RecipeBookFk == bookId && row.RowKind == SearchExportRowKind.Book
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, 0)
                        .SetProperty(e => e.NextRetryTime, (long?)null)
                        .SetProperty(e => e.ExtractionStatus, e => status),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<SuccessResult<RecipeBookDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeBookAsync(long bookId, long userId, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);
        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        return new SuccessResult<RecipeBookDao>(
            ToRecipeBookDao(bookQuery.Book, bookQuery.PermissionFlags)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > ListRecipeBooksAsync(long userId, ListRecipeBooksArgs listArgs, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        IQueryable<AdditionalBookUserAccessDbObject> accesses =
            dbContext.RecipeBookAccessEntries.Where(a => a.UserFk == userId);
        IQueryable<RecipeBookDbObject> books = dbContext.RecipeBooks.Where(b =>
            (
                b.OwnerFk == userId
                || b.AdditionalUsers.Any(u => u.MayViewBook && u.Reviewed && u.UserFk == userId)
            ) && !b.IsDeleted
        );

        var query =
            from book in books
            join access in accesses on book.Id equals access.RecipeBookFk into accessGroup
            from access in accessGroup.DefaultIfEmpty()
            select new { Book = book, Access = access };

        if (listArgs.AfterBookKey is long afterBookId)
        {
            if (afterBookId < 0)
            {
                return new ValidationFailureResult();
            }
            query = query.Where(r => r.Book.Id > afterBookId);
        }

        if (listArgs.BeforeBookKey is long beforeBookId)
        {
            if (beforeBookId < 0)
            {
                return new ValidationFailureResult();
            }
            query = query.Where(r => r.Book.Id < beforeBookId);
        }

        if (listArgs.ResultOrder == RecordOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Book.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Book.Id);
        }

        return new SuccessResult<List<RecipeBookDao>>(
            await query
                .Take(listArgs.ResultCount)
                .AsAsyncEnumerable()
                .Select(row =>
                    RecipeBookQueryExtensions.MaterializeBookAndPermissions(
                        row.Book,
                        userId,
                        row.Access
                    )
                )
                .Select(bookAndPermissions =>
                    ToRecipeBookDao(bookAndPermissions.Book, bookAndPermissions.PermissionFlags)
                )
                .ToListAsync(ct)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateRecipeBookAsync(
        long bookId,
        long userId,
        UpdateRecipeBookArgs updateArgs,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userId);
        }

        UpdateRecipeBookArgsValidator validation = new();
        ValidationResult validationResult = await validation.ValidateAsync(updateArgs, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct, withTracking: true);
        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayEditBook)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        RecipeBookDbObject book = bookQuery.Book;
        if (book.ConcurrencyTag != updateArgs.ConcurrencyTag)
        {
            return new ConflictResult();
        }

        MarkBookDirty(book);
        book.Name = updateArgs.Name;
        book.ShortDescription = updateArgs.ShortDescription;

        try
        {
            using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(ct);

            await ResetExtractionStatusAsync(
                bookId,
                dbContext,
                SearchExtractionStatus.PendingExtraction,
                ct
            );
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictResult();
        }

        recipeSearchExportStatusRepository.SignalExtractionWork();
        return new SuccessResult<RecipeBookDao>(ToRecipeBookDao(book, bookQuery.PermissionFlags));
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateShareKeyAsync(
        long bookId,
        long userId,
        string ocTag,
        BookShareKeyUpdateKind updateKind,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct, withTracking: true);
        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayManageAccess)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        RecipeBookDbObject book = bookQuery.Book;
        if (book.ConcurrencyTag != ocTag)
        {
            return new ConflictResult();
        }

        MarkBookDirty(book);
        book.ShareKey =
            updateKind == BookShareKeyUpdateKind.Regenerate
                ? RandomNumberGenerator.GetHexString(32, true)
                : "";
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictResult();
        }

        return new SuccessResult<RecipeBookDao>(ToRecipeBookDao(book, bookQuery.PermissionFlags));
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            NotFoundResult,
            SuccessResult<List<RecipeBookUserPermissionsDao>>,
            UserNotFoundResult,
            ForbiddenResult
        >
    > ListUsersWithAccess(long bookId, long userId, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayManageAccess)
        {
            return new ForbiddenResult();
        }

        List<RecipeBookUserPermissionsDao> result = [];
        await foreach (
            var row in dbContext
                .RecipeBookAccessEntries.AsNoTracking()
                .ScopeToBook(bookId)
                .Select(u => new
                {
                    u.UserFk,
                    u.MayViewBook,
                    u.MayEditBook,
                    u.Reviewed,
                    u.User!.DisplayName,
                })
                .AsAsyncEnumerable()
                .WithCancellation(ct)
        )
        {
            result.Add(
                new()
                {
                    UserId = row.UserFk,
                    BookId = bookId,
                    MayEditBook = row.MayEditBook,
                    MayViewBook = row.MayViewBook,
                    UserDisplayName = row.DisplayName,
                    Reviewed = row.Reviewed,
                }
            );
        }

        return new SuccessResult<List<RecipeBookUserPermissionsDao>>(result);
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateUsersAccessAsync(
        long bookId,
        long userId,
        IEnumerable<KeyValuePair<long, UpdateRecipeBookUserPermissionArgs?>> args,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? bookQuery =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayManageAccess)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        long now = clock.NowUtcUnixSeconds();
        bool anyChanges = false;
        foreach (KeyValuePair<long, UpdateRecipeBookUserPermissionArgs?> kv in args)
        {
            if (kv.Key == bookQuery.Book.OwnerFk)
            {
                return new ValidationFailureResult();
            }

            if (!await dbContext.Users.UserExistsNotDeletedAsync(kv.Key, ct))
            {
                return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                    new UserNotFoundResult(kv.Key);
            }

            AdditionalBookUserAccessDbObject? existingRow = await dbContext
                .RecipeBookAccessEntries.ScopeToBook(bookId)
                .WithUserId(kv.Key)
                .FirstOrDefaultAsync(ct);

            // block adding new user entries through this API to mirror web
            // contracts.
            if (existingRow is null && kv.Value is not null)
            {
                return new ValidationFailureResult();
            }
            else if (existingRow is not null)
            {
                if (kv.Value is not null)
                {
                    bool nextReviewFlag = existingRow.Reviewed || kv.Value.Reviewed; // reviewed flag is one-way
                    if (
                        existingRow.MayEditBook != kv.Value.MayEditBook
                        || existingRow.MayViewBook != kv.Value.MayViewBook
                        || existingRow.Reviewed != nextReviewFlag
                    )
                    {
                        existingRow.MayEditBook = kv.Value.MayEditBook;
                        existingRow.MayViewBook = kv.Value.MayViewBook;
                        existingRow.Reviewed = nextReviewFlag;
                        existingRow.LastModified = now;
                        existingRow.ConcurrencyTag = concurrencyTagProvider.NextTag();
                        anyChanges = true;
                    }
                }
                else
                {
                    anyChanges = true;
                    dbContext.Remove(existingRow);
                }
            }
        }

        if (!anyChanges)
        {
            return new EmptySuccessResult();
        }

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        return new EmptySuccessResult();
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > GetAccessStatusAsync(long bookId, long userId, string? shareKey, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? queryResult =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return new ValidationFailureResult();
        }

        RecipeBookDbObject bookDbObject = queryResult.Book;
        AdditionalBookUserAccessDbObject? userAccessEntry = queryResult.AccessEntry;

        // access entry does not exist or the share key is not valid
        if (
            userAccessEntry is null
            && (
                string.IsNullOrWhiteSpace(bookDbObject.ShareKey)
                || shareKey != bookDbObject.ShareKey
            )
        )
        {
            return new NotFoundResult();
        }

        // entry was reviewed and the user should have no access
        if (userAccessEntry is not null && userAccessEntry.Reviewed && !userAccessEntry.MayViewBook)
        {
            return new NotFoundResult();
        }

        return new SuccessResult<RecipeBookAccessRequestStatus>(
            new RecipeBookAccessRequestStatus(bookId, bookDbObject, userAccessEntry)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            NotFoundResult,
            ValidationFailureResult,
            ConflictResult,
            UserNotFoundResult
        >
    > RequestAccessAsync(long bookId, long userId, string shareKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(shareKey))
        {
            return new ValidationFailureResult();
        }

        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? queryResult =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return new ValidationFailureResult();
        }

        if (queryResult.AccessEntry is not null)
        {
            // owner said user gets no access which means pretend it does not exist
            if (queryResult.AccessEntry.Reviewed && !queryResult.AccessEntry.MayViewBook)
            {
                return new NotFoundResult();
            }

            // no-op - return existing state
            return new SuccessResult<RecipeBookAccessRequestStatus>(
                new RecipeBookAccessRequestStatus(bookId, queryResult.Book, queryResult.AccessEntry)
            );
        }

        // share key has to match to send a request
        if (
            string.IsNullOrWhiteSpace(queryResult.Book.ShareKey)
            || queryResult.Book.ShareKey != shareKey
        )
        {
            return new NotFoundResult();
        }

        long now = clock.NowUtcUnixSeconds();
        AdditionalBookUserAccessDbObject newEntry = new()
        {
            LastModified = now,
            Created = now,
            RecipeBookFk = bookId,
            UserFk = userId,
            Reviewed = false,
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
        };
        dbContext.Add(newEntry);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        return new SuccessResult<RecipeBookAccessRequestStatus>(
            new RecipeBookAccessRequestStatus(bookId, queryResult.Book, newEntry)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult,
            ConflictResult
        >
    > WithdrawAccessAsync(long bookId, long userId, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        BookWithMaterializedPermissions? queryResult =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct, withTracking: true);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return new ValidationFailureResult();
        }

        if (
            queryResult.AccessEntry is null
            // block users from removing their access entry if owner specifically gave
            // them no permissions. Do this to prevent abuse. Otherwise they can delete
            // their entry and resubmit again and again harassing owner.
            || (queryResult.AccessEntry.Reviewed && !queryResult.AccessEntry.MayViewBook)
        )
        {
            return new NotFoundResult();
        }

        dbContext.Remove(queryResult.AccessEntry);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }
        return new EmptySuccessResult();
    }

    /// <summary>
    /// Helper method to mark a book as dirty
    /// </summary>
    /// <param name="book">the book to mark</param>
    /// <param name="now">the now instant if one is already in use for the whole operation</param>
    private void MarkBookDirty(RecipeBookDbObject book, long? now = null)
    {
        book.LastModified = now ?? clock.NowUtcUnixSeconds();
        book.ConcurrencyTag = concurrencyTagProvider.NextTag();
    }

    private static RecipeBookDao ToRecipeBookDao(
        RecipeBookDbObject bookDbObject,
        BookPermissionFlags permissions
    )
    {
        return new()
        {
            Id = bookDbObject.Id,
            Name = bookDbObject.Name,
            ConcurrencyTag = bookDbObject.ConcurrencyTag,
            ShortDescription = bookDbObject.ShortDescription,
            OwningUserId = bookDbObject.OwnerFk,
            LastModified = Instant.FromUnixTimeSeconds(bookDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(bookDbObject.Created),
            ShareKey = bookDbObject.ShareKey,
            MayEditBook = permissions.MayEditBook,
            MayDeleteBook = permissions.MayDeleteBook,
            MayManageAccess = permissions.MayManageAccess,
            MayShareBook = permissions.MayShareBook,
        };
    }

    /// <inheritdoc />
    public async Task<int> QueueAllRecipesBooksOwnedByUserForDeletionAsync(
        long userId,
        CancellationToken ct
    )
    {
        long now = clock.NowUtcUnixSeconds();
        string token = concurrencyTagProvider.NextTag();
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        int count = await dbContext
            .RecipeBooks.Where(b => b.OwnerFk == userId && !b.IsDeleted)
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(p => p.IsDeleted, true)
                        .SetProperty(p => p.DeletedTime, now)
                        .SetProperty(p => p.LastModified, now)
                        .SetProperty(p => p.ConcurrencyTag, token),
                ct
            );

        if (count > 0)
        {
            SignalDeletion();
        }
        return count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteAllRecipesBookAccessEntriesOwnedByUserAsync(
        long userId,
        CancellationToken ct
    )
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .RecipeBookAccessEntries.Where(acl => acl.UserFk == userId)
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> HasAnyBooksAsync(long userId, CancellationToken ct)
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext.RecipeBooks.Where(b => b.OwnerFk == userId).AnyAsync(ct);
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, SuccessResult<RecipeBookDeletionQueueEntry>>
    > GetNextBookQueuedForDeletionAsync(CancellationToken ct)
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);

        var now = clock.NowUtcUnixSeconds();
        // get next book off the delete queue
        var book = await dbContext
            .RecipeBooks.IsDeleted()
            .Where(book => book.NextDeletePoll < now)
            .OrderBy(r => r.NextDeletePoll)
            .ThenBy(r => r.Id)
            .Select(b => new { b.Id, b.ConcurrencyTag })
            .FirstOrDefaultAsync(ct);

        if (book is null)
        {
            return new EmptySuccessResult();
        }

        return new SuccessResult<RecipeBookDeletionQueueEntry>(
            new RecipeBookDeletionQueueEntry()
            {
                Id = book.Id,
                ConcurrencyTag = book.ConcurrencyTag,
            }
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult>
    > RequeueRecipesBooksForDeletionAsync(
        long bookId,
        string concurrencyToken,
        CancellationToken ct
    )
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        var queryData = await dbContext
            .RecipeBooks.WithBookId(bookId)
            .IsDeleted()
            .WithConcurrencyTag(concurrencyToken)
            .Select(b => new { b.NextDeletePoll, b.NextDeletePollCounter })
            .FirstOrDefaultAsync(ct);

        if (queryData is null)
        {
            return new ConflictResult();
        }

        long now = clock.NowUtcUnixSeconds();
        var nextDeletePollCounter = Math.Min(16, queryData.NextDeletePollCounter + 1);
        var nextDeletePoll = now + (1 << (int)queryData.NextDeletePollCounter);
        var nextConcurrencyTag = concurrencyTagProvider.NextTag();

        return
            await dbContext
                .RecipeBooks.WithBookId(bookId)
                .IsDeleted()
                .WithConcurrencyTag(concurrencyToken)
                .ExecuteUpdateAsync(
                    p =>
                        p.SetProperty(b => b.NextDeletePoll, nextDeletePoll)
                            .SetProperty(b => b.ConcurrencyTag, nextConcurrencyTag)
                            .SetProperty(b => b.LastModified, now),
                    ct
                ) > 0
            ? new EmptySuccessResult()
            : new ConflictResult();
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult>
    > PurgeRecipeBookAsync(long bookId, string concurrencyToken, CancellationToken ct)
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);

        // check if the books exists, has matching concurrency token,
        // and is deleted
        if (
            !await dbContext
                .RecipeBooks.WithBookId(bookId)
                .IsDeleted()
                .WithConcurrencyTag(concurrencyToken)
                .AnyAsync(ct)
        )
        {
            return new ConflictResult();
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await dbContext
                .RecipeBookAccessEntries.Where(access => access.RecipeBookFk == bookId)
                .ExecuteDeleteAsync(ct);
            await dbContext.RecipeBooks.WithBookId(bookId).ExecuteDeleteAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        return new EmptySuccessResult();
    }
}
