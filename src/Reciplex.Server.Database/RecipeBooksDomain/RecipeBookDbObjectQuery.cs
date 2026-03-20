using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database.RecipeBooksDomain;

class RecipeBookDbObjectQuery(ApplicationDbContext dbContext)
{
    public async Task<RecipeBookDbObjectQueryResult?> ExecuteQueryAsync(
        long bookId,
        long userId,
        CancellationToken cancellationToken,
        bool? asNoTracking = null
    )
    {
        var book = await (
            asNoTracking == true ? dbContext.RecipeBooks.AsNoTracking() : dbContext.RecipeBooks
        )
            .Where(b =>
                b.Id == bookId
                && b.Deleted == null
                && (
                    b.OwnerFk == userId
                    || b.AdditionalUsers.Any(access =>
                        access.MayViewBook
                        && access.UserFk == userId
                        && access.User != null
                        && access.User.Deleted == null
                    )
                )
            )
            .OwningUserNotDeleted()
            .Include(b => b.AdditionalUsers.Where(u => u.UserFk == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (book is null)
        {
            return null;
        }

        bool mayEdit = false;
        bool mayDelete = false;
        if (book.OwnerFk == userId)
        {
            mayDelete = true;
            mayEdit = true;
        }
        else if (book.OwnerFk != userId)
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
}
