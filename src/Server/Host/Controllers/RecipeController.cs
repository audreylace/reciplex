using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime.Text;
using Reciplex.Server.Host.Models.HttpPrimitives;
using Reciplex.Server.Host.Models.PagingUtils;
using Reciplex.Server.Host.Models.Recipe;
using Reciplex.Server.Host.Utils;
using Reciplex.Server.Host.Utils.HttpResults;
using Reciplex.Server.Host.Utils.UserKeyUtils;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.Recipes;
using Reciplex.Server.RecipeServices.Recipes.Models;
using Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;
using Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;
using Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.Controllers;

using SendRecipeResults = Results<ProblemHttpResult, Ok<SingleRecipeResponseJson>>;

/// <summary>
/// Endpoints for working with a recipes
/// </summary>
/// <param name="recipeService">Provides services for getting recipe data</param>
/// <param name="recipeBookService">Provides services for get recipe books</param>
/// <param name="userService">Provides services for get user data</param>
/// <param name="recipeBookProblemFactory">factory for creating recipe book problem results</param>
/// <param name="recipeProblemFactory">factory for creating recipe problem results</param>
[ApiController]
[Route("recipes")]
[TypeFilter(typeof(InternalServerErrorOnException))]
public class RecipesController(
    IRecipeService recipeService,
    IRecipeBookService recipeBookService,
    IUserService userService,
    IRecipeProblemFactory recipeProblemFactory,
    IRecipeBookProblemFactory recipeBookProblemFactory
) : ControllerBase
{
    /// <summary>
    /// Gets a recipe by id
    /// <br />
    /// <pre><code>
    /// GET /recipes/{recipeKey}
    ///
    /// </code></pre>
    /// </summary>
    /// <param name="recipeKey">the recipe key from the request</param>
    /// <param name="userClaimsPrincipal">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpGet("{recipeKey}")]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<SendRecipeResults> GetRecipeById(
        [FromRoute] [BindRequired] RecipeKey recipeKey,
        [UseModelBinderProvider] ApplicationClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        RecipeDao? recipe = await recipeService.GetRecipeAsync(
            recipeKey,
            userClaimsPrincipal.UserKey,
            cancellationToken
        );
        if (recipe is null)
        {
            return recipeProblemFactory.RecipeNotFoundResult(recipeKey);
        }

        return await SendRecipe(recipe, userClaimsPrincipal.UserKey, cancellationToken);
    }

    /// <summary>
    /// Deletes a recipe by id
    /// <br />
    /// <pre><code>
    /// DELETE /recipes/{recipeKeys}
    /// If-Match: {ifMatch}
    ///
    /// </code></pre>
    /// </summary>
    /// <param name="recipeKey">the recipe ID from the request</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpDelete("{recipeKey}")]
    public async Task<Results<NoContent, ProblemHttpResult>> DeleteRecipeById(
        [FromRoute] [BindRequired] RecipeKey recipeKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [UseModelBinderProvider] ApplicationClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var deleteResult = await recipeService.DeleteRecipeAsync(
            recipeKey,
            userClaimsPrincipal.UserKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult.Outcome switch
        {
            DeleteRecipeByIdResultOutcome.Success => TypedResults.NoContent(),
            DeleteRecipeByIdResultOutcome.NotFound => recipeProblemFactory.RecipeNotFoundResult(
                recipeKey
            ),
            DeleteRecipeByIdResultOutcome.LacksPermission =>
                recipeProblemFactory.OperationOnRecipeForbidden(recipeKey),
            DeleteRecipeByIdResultOutcome.ConcurrencyConflict =>
                recipeProblemFactory.RecipePreconditionFailed(recipeKey, "If-Match"),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(DeleteRecipeByIdResultOutcome)} branch {deleteResult.Outcome}"
            ),
        };
    }

    /// <summary>
    /// Updates a recipe by id
    /// <br />
    /// <pre><code>
    /// PUT /recipes/{recipeKey}
    /// If-Match: {ifMatch}
    ///
    /// {
    ///     "name": "... recipe name ...",
    ///     "shortDescription": " ... recipe short description ... ",
    ///     "details": " ... recipe instructions and other details in markdown ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="recipeKey">the recipe key from the request</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="body">Request body with new recipe data</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpPut("{recipeKey}")]
    public async Task<
        Results<NoContent, ProblemHttpResult, ValidationProblem, SendRecipeResults>
    > UpdateRecipeById(
        [FromRoute] [BindRequired] RecipeKey recipeKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromBody] CreateOrUpdateRecipeJsonBody body,
        [UseModelBinderProvider] ApplicationClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        UpdateRecipeResult updateResult = await recipeService.UpdateRecipeAsync(
            recipeKey,
            userClaimsPrincipal.UserKey,
            ifMatch.Value,
            new()
            {
                Name = body.Name,
                ShortDescription = body.ShortDescription,
                Details = body.Details,
            },
            cancellationToken
        );

        return updateResult.Outcome switch
        {
            UpdateRecipeResultOutcome.ConcurrencyConflict =>
                recipeProblemFactory.RecipePreconditionFailed(recipeKey, "If-Match"),
            UpdateRecipeResultOutcome.NotFound => recipeProblemFactory.RecipeNotFoundResult(
                recipeKey
            ),
            UpdateRecipeResultOutcome.LacksPermission =>
                recipeProblemFactory.OperationOnRecipeForbidden(recipeKey),
            UpdateRecipeResultOutcome.ValidationFailure =>
                recipeProblemFactory.RecipeValidationProblem(recipeKey, updateResult.Errors),
            UpdateRecipeResultOutcome.Success => await SendRecipe(
                updateResult.Recipe,
                userClaimsPrincipal.UserKey,
                cancellationToken
            ),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(UpdateRecipeResultOutcome)} branch {updateResult.Outcome}"
            ),
        };
    }

    /// <summary>
    /// Creates a recipe
    /// <br />
    /// <pre><code>
    /// POST /recipes?book={book id}
    ///
    /// {
    ///     "name": "... recipe name ...",
    ///     "shortDescription": " ... recipe short description ... ",
    ///     "details": " ... recipe instructions and other details in markdown ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="body">Request body with new recipe data</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpPost]
    public async Task<
        Results<ProblemHttpResult, ValidationProblem, SendRecipeResults>
    > CreateRecipe(
        [FromBody] CreateOrUpdateRecipeJsonBody body,
        [BindRequired] [FromQuery(Name = "book")] RecipeBookKey bookKey,
        [UseModelBinderProvider] ApplicationClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        CreateRecipeResult createRecipeResult = await recipeService.CreateRecipeAsync(
            bookKey,
            userClaimsPrincipal.UserKey,
            new()
            {
                Name = body.Name,
                ShortDescription = body.ShortDescription,
                Details = body.Details,
            },
            cancellationToken
        );

        return createRecipeResult.Outcome switch
        {
            CreateRecipeResultOutcome.NotFound => recipeBookProblemFactory.BookNotFoundResult(
                bookKey
            ),
            CreateRecipeResultOutcome.LacksPermission =>
                recipeBookProblemFactory.OperationOnBookForbidden(bookKey),
            CreateRecipeResultOutcome.ValidationFailure =>
                recipeProblemFactory.RecipeValidationProblem(null, createRecipeResult.Errors),
            CreateRecipeResultOutcome.Success => await SendRecipe(
                createRecipeResult.Recipe,
                userClaimsPrincipal.UserKey,
                cancellationToken
            ),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(CreateRecipeResultOutcome)} branch {createRecipeResult.Outcome}"
            ),
        };
    }

    private async Task<SendRecipeResults> SendRecipe(
        RecipeDao recipe,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        var book =
            await recipeBookService.GetRecipeBookAsync(recipe.BookId, userKey, cancellationToken)
            ?? throw new Exception($"expected book {recipe.BookId} to exist for {recipe.Id}");
        var userDao =
            await userService.GetUserAsync(userKey, cancellationToken)
            ?? throw new Exception($"expected user {userKey} to exist for {book.Id}");

        return TypedResults.Ok(
            new SingleRecipeResponseJson()
            {
                Recipe = new(recipe),
                RecipeBook = new(book),
                RecipeBookOwner = new(userDao),
            }
        );
    }

    /// <summary>
    /// Path to get previous or next page:
    /// <pre><code>
    /// GET /recipes?index={page index from last query}&amp;going={forward | backward}&amp;book-id={book id}
    /// </code></pre>
    /// <br />
    /// Path to get first page:
    /// <pre><code>
    /// GET /recipes?book-id={book id}
    /// </code></pre>
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cursor"></param>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<
        Results<ValidationProblem, ProblemHttpResult, Ok<RecipePageResponseJson>>
    > GetRecipes(
        [UseModelBinderProvider] ApplicationClaimsPrincipal userClaimsPrincipal,
        [FromQuery] PageCursor<RecipeKey?> cursor,
        [FromQuery(Name = "page-size")] int? pageSize,
        [BindRequired] [FromQuery(Name = "book")] RecipeBookKey bookId,
        CancellationToken cancellationToken
    )
    {
        pageSize = Math.Min(20, Math.Max(1, pageSize ?? 10));
        var pageIterator = await recipeService.ListRecipesAsync(
            bookId,
            userClaimsPrincipal.UserKey,
            new()
            {
                AfterRecipeId =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Forwards
                        ? cursor.Index
                        : null,
                BeforeRecipeId =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Backwards
                        ? cursor.Index
                        : null,
                ResultOrder =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Backwards
                        ? ListRecipesOrdering.ByIdDecreasing
                        : ListRecipesOrdering.ByIdIncreasing,
                ResultCount = pageSize.Value,
            },
            cancellationToken
        );

        if (pageIterator is null)
        {
            return recipeBookProblemFactory.BookNotFoundResult(bookId);
        }

        List<RecipeJson> pageData = await pageIterator
            .Select(r => new RecipeJson(r))
            .OrderBy(recipe => recipe.RecipeKey.SurrogateKey)
            .ToListAsync(cancellationToken);

        RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
            bookId,
            userClaimsPrincipal.UserKey,
            cancellationToken
        );

        if (book is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

        UserDao? owningUser = await userService.GetUserAsync(book.OwningUserKey, cancellationToken);

        if (owningUser is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

        (bool hasNextPage, RecipeKey? idForNextPage) = await cursor.QueryForNextPage(
            r => r.RecipeKey,
            pageData,
            (id, cancellationToken) =>
                HasResultsBeyondPageQuery(
                    id,
                    bookId,
                    false,
                    userClaimsPrincipal.UserKey,
                    cancellationToken
                ),
            cancellationToken
        );

        (bool hasPreviousPage, RecipeKey? idForPreviousPage) = await cursor.QueryForPreviousPage(
            r => r.RecipeKey,
            pageData,
            (id, cancellationToken) =>
                HasResultsBeyondPageQuery(
                    id,
                    bookId,
                    true,
                    userClaimsPrincipal.UserKey,
                    cancellationToken
                ),
            cancellationToken
        );

        return TypedResults.Ok(
            new RecipePageResponseJson()
            {
                Recipes = pageData,
                RecipeBooks = [new(book)],
                Users = [new(owningUser)],
                NextPage =
                    hasNextPage && idForNextPage != null
                        ? new()
                        {
                            GoingQueryParam = new NavigationDirection()
                            {
                                Direction = NavigationDirection.DirectionValue.Forwards,
                            },
                            PageIndex = idForNextPage.Value,
                            BookId = bookId,
                        }
                        : null,
                PreviousPage =
                    hasPreviousPage && idForPreviousPage != null
                        ? new()
                        {
                            GoingQueryParam = new NavigationDirection()
                            {
                                Direction = NavigationDirection.DirectionValue.Backwards,
                            },
                            PageIndex = idForPreviousPage.Value,
                            BookId = bookId,
                        }
                        : null,
            }
        );
    }

    /// <summary>
    /// Runs a query to check if there are results outside the bounds of the page.
    /// <br />
    /// If <paramref name="isBack"/> is false then this will return true if <c>ANY RECIPE.ID GREATER THAN recipeId</c>.
    /// <br />
    /// If <paramref name="isBack"/> is true then this will return true if <c>ANY RECIPE.ID LESS THAN recipeId</c>.
    /// </summary>
    /// <param name="recipeId">The page boundary</param>
    /// <param name="bookId">The book ID to scope the recipe list to</param>
    /// <param name="userKey">The user running the query for access checks</param>
    /// <param name="isBack">True to check if there are results before a page <c>
    /// WHERE RECIPE.ID LESS THEN recipeId
    /// </c></param>
    /// <param name="cancellationToken">Cancels the operation</param>
    /// <returns>Task that resolves to the result of the query or null if the query fails</returns>
    private async Task<bool> HasResultsBeyondPageQuery(
        RecipeKey? recipeId,
        RecipeBookKey bookId,
        bool isBack,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        var pageIterator =
            await recipeService.ListRecipesAsync(
                bookId,
                userKey,
                new()
                {
                    AfterRecipeId = isBack ? null : recipeId,
                    BeforeRecipeId = isBack ? recipeId : null,
                    ResultCount = 1,
                },
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"expects {bookId} to exist and user {userKey} to have access"
            );

        await foreach (var _ in pageIterator)
        {
            return true;
        }
        return false;
    }
}
