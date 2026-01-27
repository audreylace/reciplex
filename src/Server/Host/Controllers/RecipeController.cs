using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models.Requests;
using Reciplex.Server.Host.Models.Responses;
using Reciplex.Server.Host.Services;
using Reciplex.Server.Host.Utils;
using Reciplex.Server.Host.Validation;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.Recipes;
using Reciplex.Server.RecipeServices.Recipes.Models;
using Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;
using Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;
using Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;

namespace Reciplex.Server.Host.Controllers;

using SendRecipeResults = Results<ProblemHttpResult, Ok<SingleRecipeResponseJson>>;

/// <summary>
/// Endpoints for working with a recipes
/// </summary>
/// <param name="userService">Provides services for getting user data</param>
[ApiController]
[Route("recipes")]
public class RecipesController(
    IRecipeService recipeService,
    IRecipeBookService recipeBookService,
    IUserService userService
) : ControllerBase
{
    /// <summary>
    /// Gets a recipe by id
    /// <br />
    /// <pre><code>
    /// GET /recipes/{recipeId}
    ///
    /// </code></pre>
    /// </summary>
    /// <param name="recipeId">the recipe ID from the request</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpGet("{recipeId}")]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<SendRecipeResults> GetRecipeById(
        [RequireRecipeId] string recipeId,
        User user,
        CancellationToken cancellationToken
    )
    {
        RecipeDao? recipe = await recipeService.GetRecipeByIdAsync(
            recipeId,
            user.UserId,
            cancellationToken
        );
        if (recipe is null)
        {
            return RecipeNotFoundResult(recipeId);
        }

        return await SendRecipe(recipe, user, cancellationToken);
    }

    /// <summary>
    /// Deletes a recipe by id
    /// <br />
    /// <pre><code>
    /// DELETE /recipes/{recipeId}
    /// If-Match: {ifMatch}
    ///
    /// </code></pre>
    /// </summary>
    /// <param name="recipeId">the recipe ID from the request</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpDelete("{recipeId}")]
    public async Task<Results<NoContent, ProblemHttpResult>> DeleteRecipeById(
        [RequireRecipeId] string recipeId,
        [RequiredAndValidIfMatch] [FromHeader(Name = "If-Match")] string ifMatch,
        User user,
        CancellationToken cancellationToken
    )
    {
        var deleteResult = await recipeService.DeleteRecipeByIdAsync(
            recipeId,
            user.UserId,
            ifMatch,
            cancellationToken
        );

        return deleteResult.Outcome switch
        {
            DeleteRecipeByIdResultOutcome.Success => TypedResults.NoContent(),
            DeleteRecipeByIdResultOutcome.NotFound => RecipeNotFoundResult(recipeId),
            DeleteRecipeByIdResultOutcome.LacksPermission => OperationOnRecipeForbidden(recipeId),
            DeleteRecipeByIdResultOutcome.ConcurrencyConflict => RecipePreconditionFailed(
                recipeId,
                "If-Match"
            ),
            _ => CustomProblemHttpResults.InternalServerError(),
        };
    }

    /// <summary>
    /// Updates a recipe by id
    /// <br />
    /// <pre><code>
    /// PUT /recipes/{recipeId}
    /// If-Match: {ifMatch}
    ///
    /// {
    ///     "name": "... recipe name ...",
    ///     "shortDescription": " ... recipe short description ... ",
    ///     "details": " ... recipe instructions and other details in markdown ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="recipeId">the recipe ID from the request</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="body">Request body with new recipe data</param>
    /// <param name="user">information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpPut("{recipeId}")]
    public async Task<
        Results<NoContent, ProblemHttpResult, ValidationProblem, SendRecipeResults>
    > UpdateRecipeById(
        [RequireRecipeId] string recipeId,
        [RequiredAndValidIfMatch] [FromHeader(Name = "If-Match")] string ifMatch,
        [FromBody] RecipeJsonBody body,
        User user,
        CancellationToken cancellationToken
    )
    {
        UpdateRecipeResult updateResult = await recipeService.UpdateRecipeAsync(
            recipeId,
            user.UserId,
            ifMatch,
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
            UpdateRecipeResultOutcome.NotFound => RecipeNotFoundResult(recipeId),
            UpdateRecipeResultOutcome.LacksPermission => OperationOnRecipeForbidden(recipeId),
            UpdateRecipeResultOutcome.InternalError =>
                CustomProblemHttpResults.InternalServerError(),
            UpdateRecipeResultOutcome.ValidationFailure => RecipeValidationProblem(
                recipeId,
                updateResult.Errors
            ),
            UpdateRecipeResultOutcome.Success => await SendRecipe(
                updateResult.Recipe,
                user,
                cancellationToken
            ),
            _ => CustomProblemHttpResults.InternalServerError(),
        };
    }

    /// <summary>
    /// Creates a recipe
    /// <br />
    /// <pre><code>
    /// POST /recipes
    ///
    /// {
    ///     "bookId": "... the book id the recipe is getting added to ... ",
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
        [FromBody] CreateRecipeJsonBody body,
        User user,
        CancellationToken cancellationToken
    )
    {
        CreateRecipeResult createRecipeResult = await recipeService.CreateRecipeAsync(
            body.BookId,
            user.UserId,
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
            CreateRecipeResultOutcome.NotFound => RecipeBooksController.BookNotFoundResult(
                body.BookId
            ),
            CreateRecipeResultOutcome.LacksPermission =>
                RecipeBooksController.OperationOnBookForbidden(body.BookId),
            CreateRecipeResultOutcome.ValidationFailure => RecipeValidationProblem(
                null,
                createRecipeResult.Errors
            ),
            CreateRecipeResultOutcome.Success => await SendRecipe(
                createRecipeResult.Recipe,
                user,
                cancellationToken
            ),
            _ => CustomProblemHttpResults.InternalServerError(),
        };
    }

    private async Task<SendRecipeResults> SendRecipe(
        RecipeDao recipe,
        User user,
        CancellationToken cancellationToken
    )
    {
        var book = await recipeBookService.GetRecipeBookAsync(
            recipe.BookId,
            user.UserId,
            cancellationToken
        );

        if (book is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

        var userDao = await userService.GetUserAsync(user.UserId, cancellationToken);
        if (userDao is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

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
    /// GET /recipes?index={page index from last query}&amp;going={forward | backward}&amp;book-id={book id, optional to scope the list to a book}
    /// </code></pre>
    /// <br />
    /// Path to get first page:
    /// <pre><code>
    /// GET /recipes?book-id={book id, optional to scope the list to a book}
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
        User user,
        PageCursor cursor,
        [NotEmptyNorWhitespace] [FromQuery(Name = "book-id")] string? bookId,
        CancellationToken cancellationToken
    )
    {
        var pageIterator = await recipeService.ListRecipesAsync(
            user.UserId,
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
                ResultCount = 10,
                BookId = bookId,
            },
            cancellationToken
        );

        if (pageIterator is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

        List<RecipeDao> pageData = await pageIterator
            .OrderBy(recipe => recipe.Id)
            .ToListAsync(cancellationToken);

        List<RecipeBookDao> books = [];
        HashSet<string> userIdsToFetch = [];
        foreach (string idOfBookToFetch in pageData.GroupBy(r => r.BookId).Select(g => g.Key))
        {
            RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
                idOfBookToFetch,
                user.UserId,
                cancellationToken
            );
            if (book is null)
            {
                continue;
            }
            books.Add(book);
            userIdsToFetch.Add(book.OwnerUserId);
        }

        bool? hasNextPage;
        string? idForNextPage = cursor.ComputeIdForNextPage(r => r.Id, pageData);
        if (cursor.ShouldRunNextPageQuery(idForNextPage))
        {
            hasNextPage = await HasResultsBeyondPageQuery(
                idForNextPage,
                bookId,
                false,
                user,
                cancellationToken
            );
            if (hasNextPage is null)
            {
                return CustomProblemHttpResults.InternalServerError();
            }
        }

        bool? hasPreviousPage;
        string? idForPreviousPage = cursor.ComputeIdForPreviousPage(r => r.Id, pageData);
        if (cursor.ShouldRunPreviousPageQuery(idForPreviousPage))
        {
            hasPreviousPage = await HasResultsBeyondPageQuery(
                idForPreviousPage,
                bookId,
                true,
                user,
                cancellationToken
            );
            if (hasPreviousPage is null)
            {
                return CustomProblemHttpResults.InternalServerError();
            }
        }

        return TypedResults.Ok(
            new RecipePageResponseJson()
            {
                Recipes = [.. pageData.Select(r => new RecipeJson(r))],
                RecipeBooks = [.. books.Select(b => new RecipeBookJson(b))],
                Users = await CommonQueries
                    .FetchUsersAsync(userIdsToFetch, userService, cancellationToken)
                    .Select(u => new UserJson(u))
                    .ToListAsync(cancellationToken),
                NextPage =
                    idForNextPage != null
                        ? new()
                        {
                            GoingQueryParam = NavigationDirection.ToQueryStringParameterValue(
                                NavigationDirection.DirectionValue.Forwards
                            ),
                            PageIndex = idForNextPage,
                            BookId = bookId,
                        }
                        : null,
                PreviousPage =
                    idForPreviousPage != null
                        ? new()
                        {
                            GoingQueryParam = NavigationDirection.ToQueryStringParameterValue(
                                NavigationDirection.DirectionValue.Backwards
                            ),
                            PageIndex = idForPreviousPage,
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
    /// <param name="user">The user running the query for access checks</param>
    /// <param name="isBack">True to check if there are results before a page <c>
    /// WHERE RECIPE.ID LESS THEN recipeId
    /// </c></param>
    /// <param name="cancellationToken">Cancels the operation</param>
    /// <returns>Task that resolves to the result of the query or null if the query fails</returns>
    private async Task<bool?> HasResultsBeyondPageQuery(
        string? recipeId,
        string? bookId,
        bool isBack,
        User user,
        CancellationToken cancellationToken
    )
    {
        var pageIterator = await recipeService.ListRecipesAsync(
            user.UserId,
            new()
            {
                AfterRecipeId = isBack ? null : recipeId,
                BeforeRecipeId = isBack ? recipeId : null,
                ResultCount = 1,
                BookId = bookId,
            },
            cancellationToken
        );

        if (pageIterator is null)
        {
            return null;
        }

        await foreach (var _ in pageIterator)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Generates a 404 error
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <returns>the 404 result</returns>
    private static ProblemHttpResult RecipeNotFoundResult(string recipeId)
    {
        return TypedResults.Problem(
            statusCode: 404,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            title: "Recipe Not Found",
            detail: $"Recipe \"{recipeId}\" was either not found or caller does not have access.",
            extensions: [new KeyValuePair<string, object?>("recipeId", recipeId)]
        );
    }

    /// <summary>
    /// Generates a 403 forbidden error
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <returns>the 404 result</returns>
    private static ProblemHttpResult OperationOnRecipeForbidden(string recipeId)
    {
        return TypedResults.Problem(
            statusCode: 403,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
            title: "Operation on Recipe Forbidden",
            detail: $"Caller lacks required permissions to perform requested action on recipe \"{recipeId}\".",
            extensions: [new KeyValuePair<string, object?>("recipeId", recipeId)]
        );
    }

    /// <summary>
    /// Generates a 412 pre-condition failure
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="failedHeader">The header that triggered the failure</param>
    /// <returns>the 412 result</returns>
    private static ProblemHttpResult RecipePreconditionFailed(string recipeId, string failedHeader)
    {
        return TypedResults.Problem(
            statusCode: 412,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-412-precondition-failed",
            title: "Pre-Condition Failure",
            detail: $"Requested operation against recipe \"{recipeId}\" failed because one or more conditions in the request headers could not be satisfied.",
            extensions:
            [
                new KeyValuePair<string, object?>("recipeId", recipeId),
                new KeyValuePair<string, object?>("failedHeader", failedHeader),
            ]
        );
    }

    /// <summary>
    /// Returns a validation problem scoped to a recipe
    /// </summary>
    /// <param name="recipeId">The recipe ID</param>
    /// <param name="errors">Map of validation errors</param>
    /// <returns>The validation problem</returns>
    private static ValidationProblem RecipeValidationProblem(
        string? recipeId,
        IDictionary<string, string[]>? errors
    )
    {
        List<KeyValuePair<string, object?>> extensions = [];
        if (recipeId is not null)
        {
            extensions.Add(new KeyValuePair<string, object?>("recipeId", recipeId));
        }
        string recipeIdPart = recipeId is not null ? $"\"{recipeId}\" " : "";
        return TypedResults.ValidationProblem(
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            title: "Validation Problem",
            detail: $"Requested operation against recipe {recipeIdPart}failed because one or more validation errors",
            errors: errors ?? new Dictionary<string, string[]>(),
            extensions: extensions
        );
    }
}
