using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;
using Reciplex.Server.RecipeServices.Utils;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class RecipeBookService(
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    ApplicationDbContext dbContext
) : IRecipeBookService
{
    public async Task<CreateRecipeBookResult> CreateRecipeBookAsync(
        UserKey userKey,
        CreateRecipeBookArgs args,
        CancellationToken cancellationToken
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        RecipeBookDbObject bookDbObject = new()
        {
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            LastModified = now,
            Created = now,
            ConcurrencyTag = concurrencyTagProvider.Next(),
            OwnerFk = userKey.SurrogateKey,
        };
        dbContext.Add(bookDbObject);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new(ToRecipeBookDao(bookDbObject, true, true));
    }

    public async Task<DeleteRecipeBookResult> DeleteRecipeBookAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    )
    {
        var bookData = await GetBookInternalAsync(bookKey, userKey, cancellationToken);
        if (bookData is null)
        {
            return new(DeleteRecipeBookResultOutcome.NotFound);
        }

        if (!bookData.MayDelete) // only the owner of the book can delete the book
        {
            return new(DeleteRecipeBookResultOutcome.LacksPermission);
        }

        var book = bookData.RecipeBook;
        if (book.ConcurrencyTag != concurrencyTag)
        {
            return new(DeleteRecipeBookResultOutcome.ConcurrencyConflict);
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
            return new(DeleteRecipeBookResultOutcome.ConcurrencyConflict);
        }

        return new(DeleteRecipeBookResultOutcome.Success);
    }

    public async Task<RecipeBookDao?> GetRecipeBookAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        var bookData = await GetBookInternalAsync(
            bookKey,
            userKey,
            cancellationToken,
            asNoTracking: true
        );
        if (bookData is null)
        {
            return null;
        }

        return ToRecipeBookDao(bookData.RecipeBook, bookData.MayEdit, bookData.MayDelete);
    }

    public async Task<IAsyncEnumerable<RecipeBookDao>> ListRecipeBooksAsync(
        UserKey userKey,
        ListRecipeBooksArgs args,
        CancellationToken cancellationToken
    )
    {
        long userId = userKey.SurrogateKey;
        return dbContext
            .RecipeBooks.AsNoTracking()
            .Where(b =>
                (
                    b.OwnerFk == userId
                    || b.AdditionalUsers.Any(access =>
                        access.MayViewBook && access.UserFk == userId
                    )
                )
                && b.Deleted == null
            )
            .Include(b => b.AdditionalUsers.Where(u => u.UserFk == userId))
            .AsAsyncEnumerable()
            .Select(book =>
            {
                bool mayEdit = false;
                bool mayDelete = false;
                if (book.OwnerFk == userKey.SurrogateKey)
                {
                    mayDelete = true;
                    mayEdit = true;
                }
                else if (book.OwnerFk != userKey.SurrogateKey)
                {
                    var userPermissions =
                        book.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId)
                        ?? throw new Exception(
                            "query broken, gave back a recipe book that the current user does not have access"
                        );

                    mayEdit = userPermissions.MayEditBook;
                }
                return ToRecipeBookDao(book, mayEdit, mayDelete);
            });
    }

    public async Task<UpdateRecipeBookDetailsResult> UpdateRecipeBookDetailsAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        UpdateRecipeBookDetailsArgs args,
        CancellationToken cancellationToken
    )
    {
        var bookData = await GetBookInternalAsync(bookKey, userKey, cancellationToken);
        if (bookData is null)
        {
            return new(UpdateRecipeBookDetailsOutcome.NotFound);
        }

        if (!bookData.MayEdit)
        {
            return new(UpdateRecipeBookDetailsOutcome.LacksPermission);
        }

        var book = bookData.RecipeBook;
        if (book.ConcurrencyTag != args.ConcurrencyTag)
        {
            return new(UpdateRecipeBookDetailsOutcome.ConcurrencyConflict);
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
            return new(UpdateRecipeBookDetailsOutcome.ConcurrencyConflict);
        }

        return new(UpdateRecipeBookDetailsOutcome.Success);
    }

    class GetBookInternalResult
    {
        public required RecipeBookDbObject RecipeBook { get; init; }
        public required bool MayEdit { get; init; }
        public required bool MayDelete { get; init; }
    }

    private async Task<GetBookInternalResult?> GetBookInternalAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        CancellationToken cancellationToken,
        bool? asNoTracking = null
    )
    {
        long userId = userKey.SurrogateKey;
        long bookId = bookKey.SurrogateKey;
        var book = await (
            asNoTracking == true ? dbContext.RecipeBooks.AsNoTracking() : dbContext.RecipeBooks
        )
            .Where(b =>
                b.Id == bookId
                && b.Deleted == null
                && (
                    b.OwnerFk == userId
                    || b.AdditionalUsers.Any(access =>
                        access.MayViewBook && access.UserFk == userId
                    )
                )
            )
            .Include(b => b.AdditionalUsers.Where(u => u.UserFk == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (book is null)
        {
            return null;
        }

        bool mayEdit = false;
        bool mayDelete = false;
        if (book.OwnerFk == userKey.SurrogateKey)
        {
            mayDelete = true;
            mayEdit = true;
        }
        else if (book.OwnerFk != userKey.SurrogateKey)
        {
            var userPermissions = book.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId);
            if (userPermissions is null || !userPermissions.MayViewBook)
            {
                return null;
            }

            mayEdit = userPermissions.MayEditBook;
        }

        return new()
        {
            MayDelete = mayDelete,
            MayEdit = mayEdit,
            RecipeBook = book,
        };
    }

    private static RecipeBookDao ToRecipeBookDao(
        RecipeBookDbObject bookDbObject,
        bool mayEdit,
        bool mayDelete
    )
    {
        return new()
        {
            Id = new(bookDbObject.Id),
            Name = bookDbObject.Name,
            ConcurrencyTag = bookDbObject.ConcurrencyTag,
            ShortDescription = bookDbObject.ShortDescription,
            OwningUserKey = new(bookDbObject.OwnerFk),
            LastModified = Instant.FromUnixTimeSeconds(bookDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(bookDbObject.Created),
            MayEditBook = mayEdit,
            MayDeleteBook = mayDelete,
        };
    }
}
