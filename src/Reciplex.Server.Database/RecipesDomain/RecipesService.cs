using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Implements <see cref="IRecipeService"/>
/// </summary>
/// <param name="dbContext">database connection</param>
/// <param name="clock">time provider</param>
/// <param name="concurrencyTagProvider">concurrency token provider</param>
/// <param name="stringIdProvider">string id marshaller</param>
/// <param name="searchExtractionOptions">options for search extraction</param>
internal sealed class RecipesService(
    ApplicationDbContext dbContext,
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    IStringIdProvider stringIdProvider,
    IOptions<SearchExtractionOptions> searchExtractionOptions
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
    > CreateRecipeAsync(string bookKey, string userKey, CreateRecipeArgs args, CancellationToken ct)
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        if (!stringIdProvider.TryParseStringKey(bookKey, out long bookId))
        {
            return new NotFoundResult();
        }

        CreateRecipeArgsValidator validator = new();
        var validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        BookWithMaterializedPermissions? bookLookup = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(bookId, userId, ct);

        if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
        {
            return new NotFoundResult();
        }
        if (!bookLookup.PermissionFlags.MayEditBook)
        {
            return new ForbiddenResult();
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        RecipeDbObject recipeDbObject = new()
        {
            RecipeBookFk = bookLookup.Book.Id,
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            Details = args.Details,
            Created = now,
            LastModified = now,
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
            SearchVersionTag = concurrencyTagProvider.NextTag(),
        };

        dbContext.Add(recipeDbObject);
        PostChangeQueueEntry(now, recipeDbObject, RecordChangeActionKind.Created);

        await dbContext.SaveChangesAsync(ct);
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
    > DeleteRecipeAsync(
        string recipeKey,
        string userKey,
        string concurrencyTag,
        CancellationToken ct
    )
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        if (!stringIdProvider.TryParseStringKey(recipeKey, out long recipeId))
        {
            return new NotFoundResult();
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.WithRecipeId(recipeId)
            .DeleteFieldNull()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }

        BookWithMaterializedPermissions? bookLookup = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(recipe.RecipeBookFk, userId, ct);

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

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        recipe.LastModified = now;
        recipe.Deleted = now;
        recipe.ConcurrencyTag = concurrencyTagProvider.NextTag();
        recipe.SearchVersionTag = concurrencyTagProvider.NextTag();
        PostChangeQueueEntry(now, recipe, RecordChangeActionKind.Deleted);
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
        DatabaseResultVariant<SuccessResult<RecipeDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeAsync(string recipeKey, string userKey, CancellationToken ct)
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        if (!stringIdProvider.TryParseStringKey(recipeKey, out long recipeId))
        {
            return new NotFoundResult();
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.AsNoTracking()
            .WithRecipeId(recipeId)
            .DeleteFieldNull()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }
        BookWithMaterializedPermissions? bookLookup = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(recipe.RecipeBookFk, userId, ct);

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
            UserNotFoundResult,
            ValidationFailureResult
        >
    > ListRecipesAsync(string userKey, ListRecipesArgs args, CancellationToken ct)
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        IQueryable<RecipeDbObject> query = dbContext
            .RecipeBooks.AsNoTracking()
            .NotDeleted()
            .UserHasAccess(userId)
            .SelectMany(b => b.Recipes)
            .DeleteFieldNull();

        if (!string.IsNullOrWhiteSpace(args.RecipeBookKey))
        {
            if (!stringIdProvider.TryParseStringKey(args.RecipeBookKey, out long bookId))
            {
                return new NotFoundResult();
            }

            BookWithMaterializedPermissions? bookLookup = await dbContext
                .RecipeBooks.AsNoTracking()
                .GetBookAndPermissionsAsync(bookId, userId, ct);

            if (bookLookup is null || !bookLookup.PermissionFlags.MayViewBook)
            {
                return new NotFoundResult();
            }

            query = query.ScopeToBook(bookId);
        }

        if (!string.IsNullOrEmpty(args.AfterRecipeKey))
        {
            if (!stringIdProvider.TryParseStringKey(args.AfterRecipeKey, out long recipeId))
            {
                return new ValidationFailureResult(
                    new Dictionary<string, string[]>()
                    {
                        {
                            "AfterRecipeId",
                            [$"value '{args.AfterRecipeKey}' is not a valid recipe id"]
                        },
                    }
                );
            }

            query = query.Where(r => r.Id > recipeId);
        }

        if (!string.IsNullOrEmpty(args.BeforeRecipeKey))
        {
            if (!stringIdProvider.TryParseStringKey(args.BeforeRecipeKey, out long recipeId))
            {
                return new ValidationFailureResult(
                    new Dictionary<string, string[]>()
                    {
                        {
                            "BeforeRecipeId",
                            [$"value '{args.BeforeRecipeKey}' is not a valid recipe id"]
                        },
                    }
                );
            }

            query = query.Where(r => r.Id < recipeId);
        }

        var ordering = args.ResultOrder;
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
                    Id = stringIdProvider.AsString(r.Id),
                    Name = r.Name,
                    ShortDescription = r.ShortDescription,
                    BookId = stringIdProvider.AsString(r.RecipeBookFk),
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
        string recipeKey,
        string userKey,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken ct
    )
    {
        if (
            !stringIdProvider.TryParseStringKey(userKey, out long userId)
            || !await dbContext.Users.UserExistsNotDeletedAsync(userId, ct)
        )
        {
            return new UserNotFoundResult(userKey);
        }

        if (!stringIdProvider.TryParseStringKey(recipeKey, out long recipeId))
        {
            return new NotFoundResult();
        }

        UpdateRecipeArgsValidator validator = new();
        var validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        RecipeDbObject? recipe = await dbContext
            .Recipes.WithRecipeId(recipeId)
            .DeleteFieldNull()
            .FirstOrDefaultAsync(ct);

        if (recipe is null)
        {
            return new NotFoundResult();
        }

        BookWithMaterializedPermissions? bookLookup = await dbContext
            .RecipeBooks.AsNoTracking()
            .GetBookAndPermissionsAsync(recipe.RecipeBookFk, userId, ct);

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

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        recipe.Name = args.Name;
        recipe.ShortDescription = args.ShortDescription;
        recipe.Details = args.Details;
        recipe.LastModified = now;
        recipe.ConcurrencyTag = concurrencyTagProvider.NextTag();
        recipe.SearchVersionTag = concurrencyTagProvider.NextTag();
        PostChangeQueueEntry(now, recipe, RecordChangeActionKind.Changed);
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        return new SuccessResult<RecipeDao>(DbObjectToRecipeDao(recipe, true));
    }

    private RecipeDao DbObjectToRecipeDao(RecipeDbObject recipeDbObject, bool mayEdit) =>
        new()
        {
            Id = stringIdProvider.AsString(recipeDbObject.Id),
            Name = recipeDbObject.Name,
            ShortDescription = recipeDbObject.ShortDescription,
            ConcurrencyTag = recipeDbObject.ConcurrencyTag,
            LastModified = Instant.FromUnixTimeSeconds(recipeDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(recipeDbObject.Created),
            BookId = stringIdProvider.AsString(recipeDbObject.RecipeBookFk),
            Details = recipeDbObject.Details,
            MayEdit = mayEdit,
        };

    /// <summary>
    /// Posts a change queue entry
    /// </summary>
    /// <param name="now">the current now timestamp</param>
    /// <param name="recipeDbObject">recipe db object</param>
    /// <param name="recordChangeActionKind">the change action kind</param>
    private void PostChangeQueueEntry(
        long now,
        RecipeDbObject recipeDbObject,
        RecordChangeActionKind recordChangeActionKind
    )
    {
        if (searchExtractionOptions.Value.Enable)
        {
            RecordDbObjectChangeEntry recordDbObjectChangeEntry = new()
            {
                RecordKind = RecordChangeSourceKind.Recipe,
                ChangeKind = recordChangeActionKind,
                Created = now,
                ObservedSearchVersionTag = recipeDbObject.SearchVersionTag,
                TargetRecipe = recipeDbObject,
            };
            recipeDbObject.ChangeQueueEntries.Add(recordDbObjectChangeEntry);
            dbContext.Add(recordDbObjectChangeEntry);
        }
    }
}
