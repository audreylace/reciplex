using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database;

namespace Recipe.Database.Queries;

public class UserHasAccessToRecipeBookQuery(ApplicationDbContext applicationDbContext)
{
    public async Task<bool> HasAccessAsync(
        long userId,
        long recipeBookId,
        CancellationToken cancellationToken
    )
    {
        return await applicationDbContext
            .RecipeBooks.Where(book =>
                book.Id == recipeBookId
                && (
                    book.OwnerFk == userId
                    || book.AdditionalUsers.Any(shared =>
                        shared.UserFk == userId && shared.ReadAccess
                    )
                )
            )
            .AnyAsync(cancellationToken);
    }
}

public class RecipeBookPermissionsResult
{
    public required long RecipeBookId { get; init; }
    public required long OwnerId { get; init; }
    public required bool HasReadAccess { get; init; }
    public required bool HasWriteAccess
    {
        get { return HasReadAccess && (field); }
        init;
    }
}

public class RecipeBookPermissionsQuery(ApplicationDbContext applicationDbContext)
{
    public async Task<RecipeBookPermissionsResult?> GetUsersPermissionsAsync(
        long userId,
        long recipeBookId,
        CancellationToken cancellationToken
    )
    {
        var result = await applicationDbContext
            .RecipeBooks.Where(book => book.Id == recipeBookId)
            .Select(book => new
            {
                book.Id,
                book.OwnerFk,
                AccessEntry = book
                    .AdditionalUsers.Where(u => u.UserFk == userId)
                    .Select(u => new { u.ReadAccess, u.WriteAccess })
                    .FirstOrDefault(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new()
        {
            RecipeBookId = userId,
            OwnerId = result.OwnerFk,
            HasReadAccess = result.OwnerFk == userId || (result.AccessEntry?.ReadAccess ?? false),
            HasWriteAccess = result.OwnerFk == userId || (result.AccessEntry?.WriteAccess ?? false),
        };
    }
}
