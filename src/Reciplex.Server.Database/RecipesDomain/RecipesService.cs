using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Implements <see cref="IRecipeService"/>
/// </summary>
/// <param name="dbFactory">database connection factory</param>
/// <param name="clock">time provider</param>
/// <param name="concurrencyTagProvider">concurrency token provider</param>
/// <param name="recipeSearchExportStatusRepository">repository for tracking recipe search export status</param>
/// <param name="searchIndexRepository">repository for searching for recipes</param>
internal sealed class RecipesService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository
) : IRecipesService
{
    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeAsync(long bookId, long userId, CreateRecipeArgs args, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        CreateRecipeArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        BookWithMaterializedPermissions? bookLookup =
            await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

        if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookLookup.PermissionFlags.MayEditBook)
        {
            return new ForbiddenResult();
        }

        long now = clock.NowUtcUnixSeconds();

        RecipeDbObject recipeDbObject = new()
        {
            RecipeBookFk = bookLookup.Book.Id,
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            Details = args.Details,
            Created = now,
            LastModified = now,
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
        };
        dbContext.Add(recipeDbObject);

        var searchStatus = new SearchExtractionStatusDbObject
        {
            Recipe = recipeDbObject,
            RecipeBookFk = bookLookup.Book.Id,
            RowKind = SearchExportRowKind.Recipe,
        };
        dbContext.Add(searchStatus);

        await dbContext.SaveChangesAsync(ct);

        recipeSearchExportStatusRepository.SignalExtractionWork();
        return new SuccessResult<RecipeDao>(DbObjectToRecipeDao(recipeDbObject, true));
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
    > QueueRecipeDeleteAsync(
        long recipeId,
        long userId,
        string concurrencyTag,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.WithRecipeId(recipeId)
            .NotDeleted()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }

        BookWithMaterializedPermissions? bookLookup =
            await dbContext.GetBookWithUserPermissionsAsync(recipe.RecipeBookFk, userId, ct);

        if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        if (!bookLookup.PermissionFlags.MayEditBook)
        {
            return new ForbiddenResult();
        }

        if (recipe.ConcurrencyTag != concurrencyTag)
        {
            return new ConflictResult();
        }

        long now = clock.NowUtcUnixSeconds();
        recipe.LastModified = now;
        recipe.IsDeleted = true;
        recipe.ConcurrencyTag = concurrencyTagProvider.NextTag();
        recipe.DeletedTime = now;

        try
        {
            using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(ct);
            await UpdateSearchExtractionStatusAsync(
                dbContext,
                recipe,
                SearchExtractionStatus.PendingDelete,
                ct
            );

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }
        recipeSearchExportStatusRepository.SignalDeletionWork();
        return new EmptySuccessResult();
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<SuccessResult<RecipeDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeAsync(long recipeId, long userId, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.AsNoTracking()
            .WithRecipeId(recipeId)
            .NotDeleted()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }

        BookWithMaterializedPermissions? bookLookup =
            await dbContext.GetBookWithUserPermissionsAsync(recipe.RecipeBookFk, userId, ct);

        if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }

        return new SuccessResult<RecipeDao>(
            DbObjectToRecipeDao(recipe, bookLookup.PermissionFlags.MayEditBook)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            NotFoundResult,
            UserNotFoundResult
        >
    > ListRecipesAsync(long userId, ListRecipesArgs args, CancellationToken ct)
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }
        IQueryable<RecipeDbObject> query;
        if (args.RecipeBookId is long bookId)
        {
            BookWithMaterializedPermissions? bookData =
                await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

            if (bookData is null)
            {
                return new NotFoundResult();
            }

            if (!bookData.PermissionFlags.MayViewBook)
            {
                return new NotFoundResult();
            }

            query = dbContext.Recipes.AsNoTracking().NotDeleted().NotDeleted().ScopeToBook(bookId);
        }
        else
        {
            query = dbContext
                .RecipeBooks.AsNoTracking()
                .NotDeleted()
                .UserHasAccess(userId)
                .SelectMany(b => b.Recipes)
                .NotDeleted();
        }

        if (args.AfterRecipeId is long afterRecipeId)
        {
            query = query.Where(r => r.Id > afterRecipeId);
        }

        if (args.BeforeRecipeId is long beforeRecipeId)
        {
            query = query.Where(r => r.Id < beforeRecipeId);
        }

        RecordOrdering? ordering = args.ResultOrder;
        if (ordering == RecordOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        return new SuccessResult<List<RecipeListEntryDao>>(
            await query
                .Select(r => new RecipeListEntryDao()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ShortDescription = r.ShortDescription,
                    BookId = r.RecipeBookFk,
                })
                .Take(args.ResultCount)
                .ToListAsync(ct)
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateRecipeAsync(
        long recipeId,
        long userId,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken ct
    )
    {
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        UpdateRecipeArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.WithRecipeId(recipeId)
            .NotDeleted()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }

        BookWithMaterializedPermissions? bookLookup =
            await dbContext.GetBookWithUserPermissionsAsync(recipe.RecipeBookFk, userId, ct);

        if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }
        if (!bookLookup.PermissionFlags.MayEditBook)
        {
            return new ForbiddenResult();
        }

        if (recipe.ConcurrencyTag != concurrencyTag)
        {
            return new ConflictResult();
        }

        recipe.Name = args.Name;
        recipe.ShortDescription = args.ShortDescription;
        recipe.Details = args.Details;
        recipe.LastModified = clock.NowUtcUnixSeconds();
        recipe.ConcurrencyTag = concurrencyTagProvider.NextTag();

        try
        {
            using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(ct);
            await UpdateSearchExtractionStatusAsync(
                dbContext,
                recipe,
                SearchExtractionStatus.PendingExtraction,
                ct
            );

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        recipeSearchExportStatusRepository.SignalExtractionWork();
        return new SuccessResult<RecipeDao>(DbObjectToRecipeDao(recipe, true));
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            FeatureNotEnabledResult,
            BookNotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > SearchRecipesAsync(long userId, SearchRecipesArgs args, CancellationToken ct)
    {
        SearchRecipesArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        if (searchIndexRepository is null || !searchIndexRepository.IsEnabled())
        {
            return new FeatureNotEnabledResult();
        }
        await using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);

        if (!await dbContext.Users.UserExistsNotDeletedAsync(userId, ct))
        {
            return new UserNotFoundResult(userId);
        }

        List<long> bookIds = [];
        if (args.Books.Count > 0)
        {
            foreach (long bookId in args.Books)
            {
                BookWithMaterializedPermissions? bookLookup =
                    await dbContext.GetBookWithUserPermissionsAsync(bookId, userId, ct);

                if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
                {
                    return new BookNotFoundResult(bookId);
                }

                bookIds.Add(bookId);
            }
        }
        else
        {
            bookIds = await dbContext
                .RecipeBooks.AsNoTracking()
                .NotDeleted()
                .UserHasAccess(userId)
                .OrderBy(book => book.Id)
                .Select(book => book.Id)
                .Take(args.MaxBooks)
                .ToListAsync(ct);
        }

        if (bookIds.Count < 1) // no books - nothing to search...
        {
            return new SuccessResult<List<RecipeListEntryDao>>([]);
        }

        List<RecipeBookSearchIndexDocument> matchesFromSearchIndex =
            await searchIndexRepository.SearchDocumentsAsync(
                new()
                {
                    SearchString = args.SearchString,
                    BookIds = bookIds,
                    MaxResults = args.MaxResults,
                },
                ct
            );

        List<RecipeListEntryDao> entries = [];
        foreach (RecipeBookSearchIndexDocument r in matchesFromSearchIndex)
        {
            if (
                r is { RecipeId: long id, Kind: RecipeBookSearchIndexDocumentKind.Recipe }
                && await dbContext.Recipes.WithRecipeId(id).NotDeleted().AnyAsync(ct)
            )
            {
                entries.Add(
                    new RecipeListEntryDao()
                    {
                        Id = id,
                        Name = r.Name,
                        ShortDescription = r.ShortDescription,
                        BookId = r.RecipeBookId,
                    }
                );
            }
        }

        return new SuccessResult<List<RecipeListEntryDao>>(entries);
    }

    /// <inheritdoc />
    public async Task<int> PurgeAllDeletedUnreferencedRecipesAsync(CancellationToken ct)
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .Recipes.Where(r =>
                !dbContext.SearchExtractionTrackers.Where(row => row.RecipeFk == r.Id).Any()
                && r.IsDeleted
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> PurgeAllDeletedUnreferencedRecipesInBookAsync(
        long bookId,
        CancellationToken ct
    )
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .Recipes.Where(r =>
                !dbContext.SearchExtractionTrackers.Where(row => row.RecipeFk == r.Id).Any()
                && r.IsDeleted
                && r.RecipeBookFk == bookId
            )
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> QueueAllRecipesInBookForDeletion(long bookId, CancellationToken ct)
    {
        long now = clock.NowUtcUnixSeconds();
        string token = concurrencyTagProvider.NextTag();
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext
            .Recipes.Where(r => r.RecipeBookFk == bookId && !r.IsDeleted)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(r => r.IsDeleted, true)
                        .SetProperty(r => r.DeletedTime, now)
                        .SetProperty(r => r.LastModified, now)
                        .SetProperty(r => r.ConcurrencyTag, token),
                ct
            );
    }

    /// <inheritdoc />
    public async Task<bool> AnyRecipesInBookAsync(long bookId, CancellationToken ct)
    {
        using ApplicationDbContext dbContext = await dbFactory.CreateDbContextAsync(ct);
        return await dbContext.Recipes.Where(r => r.RecipeBookFk == bookId).AnyAsync(ct);
    }

    /// <summary>
    /// Updates search extraction status rows for recipe object <see cref="RecipeDbObject"/>
    /// to <paramref name="newStatus"/>.
    /// </summary>
    /// <param name="dbContext">the db connection</param>
    /// <param name="recipe">the recipe db object</param>
    /// <param name="newStatus">the new status</param>
    /// <param name="ct">async cancellation token</param>
    private static async Task UpdateSearchExtractionStatusAsync(
        ApplicationDbContext dbContext,
        RecipeDbObject recipe,
        SearchExtractionStatus newStatus,
        CancellationToken ct
    )
    {
        await dbContext
            .SearchExtractionTrackers.Where(row =>
                row.RecipeBookFk == recipe.RecipeBookFk
                && row.RowKind == SearchExportRowKind.Recipe
                && row.RecipeFk == recipe.Id
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                        .SetProperty(e => e.LeaseToken, (string?)null)
                        .SetProperty(e => e.RetryCount, 0)
                        .SetProperty(e => e.NextRetryTime, (long?)null)
                        .SetProperty(e => e.ExtractionStatus, e => newStatus),
                ct
            );
    }

    /// <summary>
    /// Converts <paramref name="recipeDbObject"/> db object to <see cref="RecipeDao"/>
    /// </summary>
    /// <param name="recipeDbObject">the db object to convert</param>
    /// <param name="mayEdit">if the user is supposed to have edit status</param>
    /// <returns><see cref="RecipeDao"/> created from <paramref name="recipeDbObject"/></returns>
    private static RecipeDao DbObjectToRecipeDao(RecipeDbObject recipeDbObject, bool mayEdit) =>
        new()
        {
            Id = recipeDbObject.Id,
            Name = recipeDbObject.Name,
            ShortDescription = recipeDbObject.ShortDescription,
            ConcurrencyTag = recipeDbObject.ConcurrencyTag,
            LastModified = Instant.FromUnixTimeSeconds(recipeDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(recipeDbObject.Created),
            BookId = recipeDbObject.RecipeBookFk,
            Details = recipeDbObject.Details,
            MayEdit = mayEdit,
        };
}
