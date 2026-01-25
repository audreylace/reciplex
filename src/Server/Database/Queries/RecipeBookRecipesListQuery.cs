using Microsoft.EntityFrameworkCore;

namespace Recipe.Database.Queries;

public class RecipeBookRecipesListQuery(ApplicationDbContext applicationDbContext)
{
    public async Task<List<RecipeSummaryDao>> GetRecipesAsync(
        long recipeBookId,
        CancellationToken cancellationToken,
        RecipeBookRecipesListQueryArgs? args = null
    )
    {
        var query = applicationDbContext.Recipes.Where(r => r.RecipeBookFk == recipeBookId);

        if (args?.BeforeId is not null)
        {
            long idIndex = args.BeforeId.Value;
            query = query.Where(r => r.Id < idIndex);
        }

        if (args?.AfterId is not null)
        {
            long idIndex = args.AfterId.Value;
            query = query.Where(r => r.Id > idIndex);
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

        return await query
            .Select(r => new RecipeSummaryDao()
            {
                Id = r.Id,
                Title = r.Title,
                ShortDescription = r.ShortDescription,
            })
            .ToListAsync(cancellationToken);
    }
}
