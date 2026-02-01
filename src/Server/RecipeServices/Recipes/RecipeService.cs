using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Database;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.Recipes.Models;
using Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;
using Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;
using Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;
using Reciplex.Server.RecipeServices.Utils;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.RecipeServices.Recipes;

public class RecipeService(
    IRecipeBookService recipeBookService,
    ApplicationDbContext dbContext,
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider
) : IRecipeService
{
    /// <inheritdoc />
    public async Task<CreateRecipeResult> CreateRecipeAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    )
    {
        RecipeBookDao? bookLookup = await recipeBookService.GetRecipeBookAsync(
            bookKey,
            userKey,
            cancellationToken
        );

        if (bookLookup is null)
        {
            return new(CreateRecipeResultOutcome.NotFound);
        }

        if (!bookLookup.MayEditBook)
        {
            return new(CreateRecipeResultOutcome.LacksPermission);
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        RecipeDbObject recipeDbObject = new()
        {
            RecipeBookFk = bookKey.SurrogateKey,
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            Details = args.Details,
            Created = now,
            LastModified = now,
            ConcurrencyTag = concurrencyTagProvider.Next(),
        };
        dbContext.Add(recipeDbObject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new(DbObjectToRecipeDao(recipeDbObject, true));
    }

    public async Task<DeleteRecipeByIdResult> DeleteRecipeAsync(
        RecipeKey recipeKey,
        UserKey userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    )
    {
        var query = await RetrieveRecipeDbObject(recipeKey, userKey, cancellationToken);
        if (query is null)
        {
            return new(DeleteRecipeByIdResultOutcome.NotFound);
        }

        (RecipeDbObject recipe, bool mayEdit) = query.Value;
        if (!mayEdit)
        {
            return new(DeleteRecipeByIdResultOutcome.LacksPermission);
        }

        if (recipe.ConcurrencyTag != concurrencyTag)
        {
            return new(DeleteRecipeByIdResultOutcome.ConcurrencyConflict);
        }

        recipe.Deleted = true;
        recipe.ConcurrencyTag = concurrencyTagProvider.Next();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(DeleteRecipeByIdResultOutcome.ConcurrencyConflict);
        }

        return new(DeleteRecipeByIdResultOutcome.Success);
    }

    public async Task<RecipeDao?> GetRecipeAsync(
        RecipeKey recipeKey,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        var query = await RetrieveRecipeDbObject(
            recipeKey,
            userKey,
            cancellationToken,
            noTrack: true
        );
        if (query is null)
        {
            return null;
        }

        (RecipeDbObject recipe, bool mayEdit) = query.Value;
        return DbObjectToRecipeDao(recipe, mayEdit);
    }

    private async Task<(RecipeDbObject, bool)?> RetrieveRecipeDbObject(
        RecipeKey recipeKey,
        UserKey userKey,
        CancellationToken cancellationToken,
        bool? noTrack = false
    )
    {
        RecipeDbObject? recipe = await (
            noTrack == true ? dbContext.Recipes.AsNoTracking() : dbContext.Recipes
        )
            .Where(r => r.Id == recipeKey.SurrogateKey && r.Deleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return null;
        }

        RecipeBookDao? bookLookup = await recipeBookService.GetRecipeBookAsync(
            new(recipe.RecipeBookFk),
            userKey,
            cancellationToken
        );

        if (bookLookup is null)
        {
            return null;
        }

        return (recipe, bookLookup.MayEditBook);
    }

    public async Task<IAsyncEnumerable<RecipeDao>?> ListRecipesAsync(
        RecipeBookKey bookKey,
        UserKey userKey,
        ListRecipesArgs args,
        CancellationToken cancellationToken
    )
    {
        RecipeBookDao? bookLookup = await recipeBookService.GetRecipeBookAsync(
            bookKey,
            userKey,
            cancellationToken
        );

        if (bookLookup is null)
        {
            return null;
        }

        var query = dbContext
            .Recipes.AsNoTracking()
            .Where(r => r.Deleted == false && r.RecipeBookFk == bookKey.SurrogateKey);

        if (args.ResultOrder == ListRecipesOrdering.ByIdDecreasing)
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        if (args.AfterRecipeId is not null)
        {
            long afterRecipeId = args.AfterRecipeId.Value.SurrogateKey;
            query = query.Where(r => r.Id > afterRecipeId);
        }

        if (args.BeforeRecipeId is not null)
        {
            long beforeRecipeId = args.BeforeRecipeId.Value.SurrogateKey;
            query = query.Where(r => r.Id < beforeRecipeId);
        }

        return query
            .Take(args.ResultCount)
            .AsAsyncEnumerable()
            .Select(r => DbObjectToRecipeDao(r, bookLookup.MayEditBook));
    }

    public async Task<UpdateRecipeResult> UpdateRecipeAsync(
        RecipeKey recipeKey,
        UserKey userKey,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken cancellationToken
    )
    {
        var query = await RetrieveRecipeDbObject(recipeKey, userKey, cancellationToken);
        if (query is null)
        {
            return new(UpdateRecipeResultOutcome.NotFound);
        }

        (RecipeDbObject recipe, bool mayEdit) = query.Value;
        if (!mayEdit)
        {
            return new(UpdateRecipeResultOutcome.LacksPermission);
        }

        if (recipe.ConcurrencyTag != concurrencyTag)
        {
            return new(UpdateRecipeResultOutcome.ConcurrencyConflict);
        }

        recipe.Name = args.Name;
        recipe.ShortDescription = args.ShortDescription;
        recipe.Details = args.Details;
        recipe.LastModified = clock.GetCurrentInstant().ToUnixTimeSeconds();
        recipe.ConcurrencyTag = concurrencyTagProvider.Next();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(UpdateRecipeResultOutcome.ConcurrencyConflict);
        }

        return new(DbObjectToRecipeDao(recipe, true));
    }

    private static RecipeDao DbObjectToRecipeDao(RecipeDbObject recipeDbObject, bool mayEdit) =>
        new()
        {
            Id = new(recipeDbObject.Id),
            Name = recipeDbObject.Name,
            ShortDescription = recipeDbObject.ShortDescription,
            ConcurrencyTag = recipeDbObject.ConcurrencyTag,
            LastModified = Instant.FromUnixTimeSeconds(recipeDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(recipeDbObject.Created),
            BookId = new(recipeDbObject.RecipeBookFk),
            Details = recipeDbObject.Details,
            MayEdit = mayEdit,
        };
}
