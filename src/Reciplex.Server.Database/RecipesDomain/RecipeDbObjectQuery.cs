using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

class RecipeDbObjectQuery(ApplicationDbContext applicationDbContext)
{
    public async Task<RecipeDbObjectQueryResult> ExecuteQueryAsync(
        long recipeId,
        long userId,
        CancellationToken ct
    )
    {
        RecipeDbObject? recipeDbObject = await applicationDbContext
            .Recipes.Where(r =>
                r.Id == recipeId
                && r.Deleted == null
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
            )
            .Include(r => r.RecipeBook)
                .ThenInclude(b => b!.AdditionalUsers.Where(u => u.UserFk == userId))
            .FirstOrDefaultAsync(ct);

        if (recipeDbObject is null || recipeDbObject.RecipeBook is null)
        {
            return new RecipeDbObjectQueryResult.NotFound();
        }

        bool mayEdit = false;
        if (recipeDbObject.RecipeBook.OwnerFk == userId)
        {
            mayEdit = true;
        }
        else if (recipeDbObject.RecipeBook.OwnerFk != userId)
        {
            AdditionalBookUserAccessDbObject? userPermissions =
                recipeDbObject.RecipeBook.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId);
            if (userPermissions is null || !userPermissions.MayViewBook)
            {
                return new RecipeDbObjectQueryResult.Forbidden();
            }

            mayEdit = userPermissions.MayEditBook;
        }

        return new RecipeDbObjectQueryResult.Success(
            recipeDbObject,
            recipeDbObject.RecipeBook,
            mayEdit
        );
    }
}
