using Recipe.Database.DbObjects;

namespace Recipe.Database.Queries;

public class RecipeBookCreateCommand(ApplicationDbContext applicationDbContext)
{
    public async Task<RecipeBookDbObject> CreateBooksAsync(
        long userId,
        string name,
        string description,
        CancellationToken cancellationToken
    )
    {
        RecipeBookDbObject recipeBook = new()
        {
            Title = name,
            ShortDescription = description,
            OwnerFk = userId,
        };

        applicationDbContext.Add(recipeBook);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return recipeBook;
    }
}

public class RecipeCreateCommand(ApplicationDbContext applicationDbContext)
{
    public async Task<RecipeDbObject> CreateRecipeAsync(
        string name,
        string description,
        long recipeBookId,
        CancellationToken cancellationToken
    )
    {
        RecipeDbObject recipe = new()
        {
            Title = name,
            ShortDescription = description,
            RecipeBookFk = recipeBookId,
        };

        applicationDbContext.Add(recipe);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return recipe;
    }
}
