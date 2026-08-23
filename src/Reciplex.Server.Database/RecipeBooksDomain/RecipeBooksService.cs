using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchExtractionWorker;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipeBooksDomain;

internal sealed class RecipeBooksService(
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    ApplicationDbContext dbContext,
    IStringIdProvider stringIdProvider,
    IDurableRecordsToExtractForSearchQueue searchQueue
) : IRecipeBooksService
{
    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeBookAsync(string userKey, CreateRecipeBookArgs createArgs, CancellationToken ct)
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        CreateRecipeBookArgsValidator validation = new();
        ValidationResult validationResult = await validation.ValidateAsync(createArgs, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        RecipeBookDbObject bookDbObject = new()
        {
            Name = createArgs.Name,
            ShortDescription = createArgs.ShortDescription,
            LastModified = now,
            Created = now,
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
            OwnerFk = userId,
            SearchVersionTag = concurrencyTagProvider.NextTag(),
        };
        dbContext.Add(bookDbObject);

        await dbContext.SaveChangesAsync(ct);

        await PostChangeQueueEntryAsync(bookDbObject, RecordChangeActionKind.Created, ct);

        return new SuccessResult<RecipeBookDao>(
            ToRecipeBookDao(bookDbObject, BookPermissionFlags.OwnerPermissions())
        );
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
    > DeleteRecipeBookAsync(string bookKey, string userKey, string ocTag, CancellationToken ct)
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => (DatabaseResultVariant<
                    ForbiddenResult,
                    UserNotFoundResult
                >)
                    userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userKey);
        }

        var bookData = await dbContext.RecipeBooks.GetBookAndPermissionsAsync(bookId, userId, ct);

        if (bookData is null || !bookData.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookData.PermissionFlags.MayDeleteBook)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        var book = bookData.Book;
        if (book.ConcurrencyTag != ocTag)
        {
            return new ConflictResult();
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        MarkBookDirty(book, now);
        book.Deleted = now;
        book.SearchVersionTag = concurrencyTagProvider.NextTag();

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictResult();
        }

        await PostChangeQueueEntryAsync(book, RecordChangeActionKind.Deleted, ct);

        return new EmptySuccessResult();
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > GetRecipeBookAsync(string bookKey, string userKey, CancellationToken ct)
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        var queryResult = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (queryResult is null || !queryResult.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        return new SuccessResult<RecipeBookDao>(
            ToRecipeBookDao(queryResult.Book, queryResult.PermissionFlags)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > ListRecipeBooksAsync(string userKey, ListRecipeBooksArgs listArgs, CancellationToken ct)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        var query = dbContext.RecipeBooks.AsNoTracking().NotDeleted().UserHasAccess(userId);

        if (listArgs.AfterBookKey is not null)
        {
            if (!stringIdProvider.TryParseStringKey(listArgs.AfterBookKey, out long afterBookId))
            {
                return MakeValidationError(
                    "AfterBookKey",
                    $"Value '{listArgs.AfterBookKey}' is not a valid recipe book id"
                );
            }
            query = query.Where(r => r.Id > afterBookId);
        }

        if (listArgs.BeforeBookKey is not null)
        {
            if (!stringIdProvider.TryParseStringKey(listArgs.BeforeBookKey, out long beforeBookId))
            {
                return MakeValidationError(
                    "BeforeBookKey",
                    $"Value '{listArgs.BeforeBookKey}' is not a valid recipe book id"
                );
            }
            query = query.Where(r => r.Id < beforeBookId);
        }

        if (listArgs.ResultOrder == RecordOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        return new SuccessResult<List<RecipeBookDao>>(
            await BookDaoList(query.Take(listArgs.ResultCount), userId, ct).ToListAsync(ct)
        );
    }

    private async IAsyncEnumerable<RecipeBookDao> BookDaoList(
        IQueryable<RecipeBookDbObject> query,
        long userId,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        await foreach (var row in query.MaterializeWithPermissionsAsyncEnumerable(userId, ct))
        {
            if (!row.PermissionFlags.MayViewBook)
            {
                continue;
            }
            yield return ToRecipeBookDao(row.Book, row.PermissionFlags);
        }
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
        string bookKey,
        string userKey,
        UpdateRecipeBookArgs updateArgs,
        CancellationToken ct
    )
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => (DatabaseResultVariant<
                    ForbiddenResult,
                    UserNotFoundResult
                >)
                    userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userKey);
        }

        UpdateRecipeBookArgsValidator validation = new();
        var validationResult = await validation.ValidateAsync(updateArgs, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        var queryResult = await dbContext
            .RecipeBooks.NotDeleted()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (queryResult is null || !queryResult.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!queryResult.PermissionFlags.MayEditBook)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        var book = queryResult.Book;
        if (book.ConcurrencyTag != updateArgs.ConcurrencyTag)
        {
            return new ConflictResult();
        }

        MarkBookDirty(book);
        book.Name = updateArgs.Name;
        book.ShortDescription = updateArgs.ShortDescription;
        book.SearchVersionTag = concurrencyTagProvider.NextTag();
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictResult();
        }

        await PostChangeQueueEntryAsync(book, RecordChangeActionKind.Changed, ct);

        return new SuccessResult<RecipeBookDao>(ToRecipeBookDao(book, queryResult.PermissionFlags));
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
        string bookKey,
        string userKey,
        string ocTag,
        BookShareKeyUpdateKind updateKind,
        CancellationToken ct
    )
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => (DatabaseResultVariant<
                    ForbiddenResult,
                    UserNotFoundResult
                >)
                    userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userKey);
        }

        var queryResult = await dbContext.RecipeBooks.GetBookAndPermissionsAsync(
            bookId,
            userId,
            ct
        );

        if (queryResult is null || !queryResult.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!queryResult.PermissionFlags.MayManageAccess)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        var book = queryResult.Book;
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

        return new SuccessResult<RecipeBookDao>(ToRecipeBookDao(book, queryResult.PermissionFlags));
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            NotFoundResult,
            SuccessResult<List<RecipeBookUserPermissionsDao>>,
            UserNotFoundResult,
            ForbiddenResult
        >
    > ListUsersWithAccess(string bookKey, string userKey, CancellationToken ct)
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        var book = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (book is null || !book.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!book.PermissionFlags.MayManageAccess)
        {
            return new ForbiddenResult();
        }

        return new SuccessResult<List<RecipeBookUserPermissionsDao>>(
            await ToBookPermissionEnumerable(bookId, ct).ToListAsync(ct)
        );
    }

    private async IAsyncEnumerable<RecipeBookUserPermissionsDao> ToBookPermissionEnumerable(
        long bookId,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        string bookKey = stringIdProvider.AsString(bookId);
        await foreach (
            var row in dbContext
                .RecipeBookAccessEntries.AsNoTracking()
                .NotDeleted()
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
            yield return new()
            {
                UserKey = stringIdProvider.AsString(row.UserFk),
                BookKey = bookKey,
                MayEditBook = row.MayEditBook,
                MayViewBook = row.MayViewBook,
                UserDisplayName = row.DisplayName,
                Reviewed = row.Reviewed,
            };
        }
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
        string bookKey,
        string userKey,
        IEnumerable<KeyValuePair<string, UpdateRecipeBookUserPermissionArgs?>> args,
        CancellationToken ct
    )
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );
        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => (DatabaseResultVariant<
                    ForbiddenResult,
                    UserNotFoundResult
                >)
                    userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new UserNotFoundResult(userKey);
        }

        var bookQuery = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (bookQuery is null || !bookQuery.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookQuery.PermissionFlags.MayManageAccess)
        {
            return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                new ForbiddenResult();
        }

        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(ct);

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        bool anyChanges = false;
        foreach (KeyValuePair<string, UpdateRecipeBookUserPermissionArgs?> kv in args)
        {
            if (!stringIdProvider.TryParseStringKey(kv.Key, out long userEntryId))
            {
                return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                    new UserNotFoundResult(kv.Key);
            }

            if (userEntryId == bookQuery.Book.OwnerFk)
            {
                return ShareValidationUserOwnsBook();
            }

            if (!await dbContext.Users.UserExistsNotDeletedAsync(userEntryId, ct))
            {
                return (DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>)
                    new UserNotFoundResult(kv.Key);
            }

            var existingRow = await dbContext
                .RecipeBookAccessEntries.ScopeToBook(bookId)
                .NotDeleted()
                .WithUserId(userEntryId)
                .FirstOrDefaultAsync(ct);

            // block adding new user entries through this API to mirror web
            // contracts.
            if (existingRow is null && kv.Value is not null)
            {
                return MakeValidationError(
                    "userKey",
                    $"user '{userKey}' does not have an access entry"
                );
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
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
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
    > GetAccessStatusAsync(string bookKey, string userKey, string? shareKey, CancellationToken ct)
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );

        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult userNotFoundResult => userNotFoundResult,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        // grab book; Don't scope to user
        var queryResult = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return ShareValidationUserOwnsBook();
        }

        var bookDbObject = queryResult.Book;
        var userAccessEntry = queryResult.AccessEntry;

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
            new RecipeBookAccessRequestStatus(bookKey, bookDbObject, userAccessEntry)
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
    > RequestAccessAsync(string bookKey, string userKey, string shareKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(shareKey))
        {
            return MakeValidationError(
                "shareKey",
                "share key must not be null, empty, or whitespace"
            );
        }

        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );
        if (error is not null)
        {
            return error.Result switch
            {
                NotFoundResult v => v,
                UserNotFoundResult v => v,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        // grab book; Don't scope to user
        BookWithMaterializedPermissions? queryResult = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return ShareValidationUserOwnsBook();
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
                new RecipeBookAccessRequestStatus(
                    bookKey,
                    queryResult.Book,
                    queryResult.AccessEntry
                )
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

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
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
            new RecipeBookAccessRequestStatus(bookKey, queryResult.Book, newEntry)
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
    > WithdrawAccessAsync(string bookKey, string userKey, CancellationToken ct)
    {
        DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? error = ParseBookAndUser(
            bookKey,
            userKey,
            out long bookId,
            out long userId
        );
        if (error is not null)
        {
            return error.Result switch
            {
                UserNotFoundResult v => v,
                NotFoundResult nf => nf,
                _ => throw new NotImplementedException(),
            };
        }

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userKey);
        }

        // grab book; Don't scope to user
        BookWithMaterializedPermissions? queryResult =
            await dbContext.RecipeBooks.GetBookAndPermissionsAsync(bookId, userId, ct);

        if (queryResult is null)
        {
            return new NotFoundResult();
        }

        if (queryResult.PermissionFlags.OwnsBook)
        {
            return ShareValidationUserOwnsBook();
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
    /// Helper method to mark a book as dirt
    /// </summary>
    /// <param name="book">the book to mark</param>
    /// <param name="now">the now instant if one is already in use for the whole operation</param>
    private void MarkBookDirty(RecipeBookDbObject book, long? now = null)
    {
        book.LastModified = now ?? clock.GetCurrentInstant().ToUnixTimeSeconds();
        book.ConcurrencyTag = concurrencyTagProvider.NextTag();
    }

    private RecipeBookDao ToRecipeBookDao(
        RecipeBookDbObject bookDbObject,
        BookPermissionFlags permissions
    )
    {
        return new()
        {
            Id = stringIdProvider.AsString(bookDbObject.Id),
            Name = bookDbObject.Name,
            ConcurrencyTag = bookDbObject.ConcurrencyTag,
            ShortDescription = bookDbObject.ShortDescription,
            OwningUserKey = stringIdProvider.AsString(bookDbObject.OwnerFk),
            LastModified = Instant.FromUnixTimeSeconds(bookDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(bookDbObject.Created),
            ShareKey = bookDbObject.ShareKey,
            MayEditBook = permissions.MayEditBook,
            MayDeleteBook = permissions.MayDeleteBook,
            MayManageAccess = permissions.MayManageAccess,
            MayShareBook = permissions.MayShareBook,
        };
    }

    private static ValidationFailureResult ShareValidationUserOwnsBook()
    {
        return MakeValidationError(
            "userKey",
            "Owning user may not request access to recipe book's they own."
        );
    }

    private static ValidationFailureResult MakeValidationError(string key, string value)
    {
        return new ValidationFailureResult(new Dictionary<string, string[]>() { { key, [value] } });
    }

    private DatabaseResultVariant<UserNotFoundResult, NotFoundResult>? ParseBookAndUser(
        string bookKey,
        string userKey,
        out long bookId,
        out long userId
    )
    {
        userId = default;
        if (!stringIdProvider.TryParseStringKey(bookKey, out bookId))
        {
            return new NotFoundResult();
        }
        if (!stringIdProvider.TryParseStringKey(userKey, out userId))
        {
            return new UserNotFoundResult(userKey);
        }

        return null;
    }

    /// <summary>
    /// Posts a change queue entry
    /// </summary>
    /// <param name="recipeDbObject">book db object</param>
    /// <param name="recordChangeActionKind">the change action kind</param>
    /// <param name="ct">cancels the async operation</param>
    private async Task PostChangeQueueEntryAsync(
        RecipeBookDbObject bookDbObject,
        RecordChangeActionKind recordChangeActionKind,
        CancellationToken ct
    )
    {
        try
        {
            await searchQueue.PostBookChangeQueueEntryAsync(
                bookDbObject.Id,
                bookDbObject.SearchVersionTag,
                recordChangeActionKind,
                ct
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // todo - log?
        }
    }
}
