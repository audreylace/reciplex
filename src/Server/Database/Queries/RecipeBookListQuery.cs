using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database;
using Reciplex.Server.Database.DbObjects;

namespace Recipe.Database.Queries;

public class RecipeBookListQuery(ApplicationDbContext applicationDbContext)
{
    public async Task<List<RecipeBookDbObject>> GetBooksAsync(
        long userId,
        CancellationToken cancellationToken,
        RecipeBookListQueryArgs? args = null
    )
    {
        var query = applicationDbContext.RecipeBooks.Where(book =>
            book.OwnerFk == userId
            || book.AdditionalUsers.Any(shared => shared.UserFk == userId && shared.ReadAccess)
        );

        if (args?.BeforeId is not null)
        {
            long idIndex = args.BeforeId.Value;
            query = query.Where(book => book.Id < idIndex);
        }

        if (args?.AfterId is not null)
        {
            long idIndex = args.AfterId.Value;
            query = query.Where(book => book.Id > idIndex);
        }

        if (args?.PageSize is not null)
        {
            query = query.Take(args.PageSize.Value);
        }

        query = args?.Order switch
        {
            ResultOrdering.LargestFirst => query.OrderByDescending(book => book.Id),
            _ => query.OrderBy(book => book.Id),
        };

        return await query.ToListAsync(cancellationToken);
    }
}
