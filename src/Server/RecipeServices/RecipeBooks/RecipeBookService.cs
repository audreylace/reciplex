using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class RecipeBookService : IRecipeBookService
{
    public Task<CreateRecipeBookResult> CreateRecipeBookAsync(
        UserKey userKey,
        CreateRecipeBookArgs args,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<DeleteRecipeBookResult> DeleteRecipeBookAsync(
        RecipeBookKey bookId,
        UserKey userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<RecipeBookDao?> GetRecipeBookAsync(
        RecipeBookKey bookId,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<RecipeBookDao>> ListRecipeBooksAsync(
        UserKey userKey,
        ListRecipeBooksArgs args,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<UpdateRecipeBookDetailsResult> UpdateRecipeBookDetailsAsync(
        RecipeBookKey bookId,
        UserKey userKey,
        UpdateRecipeBookDetailsArgs args,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
