using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.RecipesDomain;

class RecipeDbObjectListQuery(ApplicationDbContext applicationDbContext)
{
    public async IAsyncEnumerable<RecipeDbObjectListEntry> ExecuteQueryAsync(
        long userId,
        long? bookId = null,
        RecordOrdering? ordering = null,
        long? idIsBefore = null,
        long? idIsAfter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var query = applicationDbContext
            .Recipes.AsNoTracking()
            .Where(r =>
                r.Deleted == null
                && r.RecipeBook != null
                && r.RecipeBook.Deleted == null
                && r.RecipeBook.Owner != null
                && r.RecipeBook.Owner.Deleted == null
                && (
                    r.RecipeBook.OwnerFk == userId
                    || r.RecipeBook.AdditionalUsers.Any(access =>
                        access.MayViewBook
                        && access.UserFk == userId
                        && access.User != null
                        && access.User.Deleted == null
                    )
                )
            );

        if (bookId is not null)
        {
            query = query.Where(r => r.RecipeBookFk == bookId);
        }

        if (ordering == RecordOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        if (idIsAfter is not null)
        {
            query = query.Where(r => r.Id > idIsAfter);
        }

        if (idIsBefore is not null)
        {
            query = query.Where(r => r.Id < idIsBefore);
        }

        await foreach (
            var row in query
                .Select(r => new
                {
                    Recipe = r,
                    MayEdit = r.RecipeBook!.OwnerFk == userId
                        || r.RecipeBook!.AdditionalUsers.Any(u =>
                            u.MayEditBook && u.UserFk == userId
                        ),
                })
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
        )
        {
            yield return new RecipeDbObjectListEntry()
            {
                Recipe = row.Recipe,
                MayEdit = row.MayEdit,
            };
        }
    }
}
