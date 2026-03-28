using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipeBooksDomain;

internal class RecipeBooksRepository(
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    ApplicationDbContext dbContext,
    IStringIdProvider stringIdProvider,
    RecipeBookDbObjectQuery recipeBookQuery
) : IRecipeBooksRepository
{
    public async Task<CreateRecipeBookResult> CreateRecipeBookAsync(
        string userKey,
        CreateRecipeBookArgs args,
        CancellationToken cancellationToken
    )
    {
        CreateRecipeBookArgsValidator validation = new();
        var validationResult = validation.Validate(args);
        if (!validationResult.IsValid)
        {
            return new CreateRecipeBookResult.ValidationFailure(validationResult.ToDictionary());
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        long userId = FromStringKey(userKey);
        RecipeBookDbObject bookDbObject = new()
        {
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            LastModified = now,
            Created = now,
            ConcurrencyTag = concurrencyTagProvider.Next(),
            OwnerFk = userId,
        };
        dbContext.Add(bookDbObject);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateRecipeBookResult.Success(ToRecipeBookDao(bookDbObject, true, true));
    }

    public async Task<DeleteRecipeBookResult> DeleteRecipeBookAsync(
        string bookKey,
        string userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    )
    {
        var bookData = await recipeBookQuery.ExecuteQueryAsync(
            FromStringKey(bookKey),
            FromStringKey(userKey),
            cancellationToken
        );
        if (bookData is null)
        {
            return new DeleteRecipeBookResult.NotFound();
        }

        if (!bookData.MayDelete) // only the owner of the book can delete the book
        {
            return new DeleteRecipeBookResult.Forbidden();
        }

        var book = bookData.RecipeBook;
        if (book.ConcurrencyTag != concurrencyTag)
        {
            return new DeleteRecipeBookResult.Conflict();
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        book.LastModified = now;
        book.Deleted = now;
        book.ConcurrencyTag = concurrencyTagProvider.Next();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new DeleteRecipeBookResult.Conflict();
        }

        return new DeleteRecipeBookResult.Success();
    }

    public async Task<RecipeBookDao?> GetRecipeBookAsync(
        string bookKey,
        string userKey,
        CancellationToken cancellationToken
    )
    {
        var bookData = await recipeBookQuery.ExecuteQueryAsync(
            FromStringKey(bookKey),
            FromStringKey(userKey),
            cancellationToken,
            asNoTracking: true
        );
        if (bookData is null)
        {
            return null;
        }

        return ToRecipeBookDao(bookData.RecipeBook, bookData.MayEdit, bookData.MayDelete);
    }

    public async IAsyncEnumerable<RecipeBookDao> ListRecipeBooksAsync(
        string userKey,
        ListRecipeBooksArgs args,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        long userId = FromStringKey(userKey);
        var query = dbContext
            .RecipeBooks.AsNoTracking()
            .Where(b =>
                (
                    b.OwnerFk == userId
                    || b.AdditionalUsers.Any(access =>
                        access.MayViewBook
                        && access.UserFk == userId
                        && access.User != null
                        && access.User.Deleted == null
                    )
                )
                && b.Deleted == null
            )
            .OwningUserNotDeleted();

        if (args.AfterBookId is not null)
        {
            long afterRecipeId = FromStringKey(args.AfterBookId);
            query = query.Where(r => r.Id > afterRecipeId);
        }

        if (args.BeforeBookId is not null)
        {
            long beforeRecipeId = FromStringKey(args.BeforeBookId);
            query = query.Where(r => r.Id < beforeRecipeId);
        }

        if (args.ResultOrder == RecordOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        await foreach (
            var row in query
                .Include(b =>
                    b.AdditionalUsers.Where(u =>
                        u.UserFk == userId && u.User != null && u.User.Deleted == null
                    )
                )
                .Take(args.ResultCount)
                .AsAsyncEnumerable()
                .Select(book =>
                {
                    bool mayEdit = false;
                    bool mayDelete = false;
                    if (book.OwnerFk == userId)
                    {
                        mayDelete = true;
                        mayEdit = true;
                    }
                    else if (book.OwnerFk != userId)
                    {
                        var userPermissions =
                            book.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId)
                            ?? throw new Exception(
                                "query broken, gave back a recipe book that the current user does not have access"
                            );

                        mayEdit = userPermissions.MayEditBook;
                    }
                    return ToRecipeBookDao(book, mayEdit, mayDelete);
                })
        )
        {
            yield return row;
        }
    }

    public async Task<UpdateRecipeBookResult> UpdateRecipeBookAsync(
        string bookKey,
        string userKey,
        UpdateRecipeBookArgs args,
        CancellationToken cancellationToken
    )
    {
        UpdateRecipeBookArgsValidator validation = new();
        var validationResult = validation.Validate(args);
        if (!validationResult.IsValid)
        {
            return new UpdateRecipeBookResult.ValidationFailure(validationResult.ToDictionary());
        }

        var bookData = await recipeBookQuery.ExecuteQueryAsync(
            FromStringKey(bookKey),
            FromStringKey(userKey),
            cancellationToken
        );
        if (bookData is null)
        {
            return new UpdateRecipeBookResult.NotFound();
        }

        if (!bookData.MayEdit)
        {
            return new UpdateRecipeBookResult.Forbidden();
        }

        var book = bookData.RecipeBook;
        if (book.ConcurrencyTag != args.ConcurrencyTag)
        {
            return new UpdateRecipeBookResult.Conflict();
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        book.LastModified = now;
        book.ConcurrencyTag = concurrencyTagProvider.Next();
        book.Name = args.Name;
        book.ShortDescription = args.ShortDescription;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new UpdateRecipeBookResult.Conflict();
        }

        return new UpdateRecipeBookResult.Success(
            ToRecipeBookDao(book, bookData.MayEdit, bookData.MayDelete)
        );
    }

    private RecipeBookDao ToRecipeBookDao(
        RecipeBookDbObject bookDbObject,
        bool mayEdit,
        bool mayDelete
    )
    {
        return new()
        {
            Id = ToStringKey(bookDbObject.Id),
            Name = bookDbObject.Name,
            ConcurrencyTag = bookDbObject.ConcurrencyTag,
            ShortDescription = bookDbObject.ShortDescription,
            OwningUserKey = ToStringKey(bookDbObject.OwnerFk),
            LastModified = Instant.FromUnixTimeSeconds(bookDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(bookDbObject.Created),
            MayEditBook = mayEdit,
            MayDeleteBook = mayDelete,
        };
    }

    private long FromStringKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        return stringIdProvider.AsLong(key)
            ?? throw new ArgumentException($"{key} does not map to a valid long", nameof(key));
    }

    private string ToStringKey(long id)
    {
        return stringIdProvider.AsString(id);
    }
}
