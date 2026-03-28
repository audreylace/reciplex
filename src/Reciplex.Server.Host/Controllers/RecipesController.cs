using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipes
/// </summary>
/// <param name="recipeService">Provides services for getting recipe data</param>
/// <param name="recipeBookService">Provides services for get recipe books</param>
/// <param name="userService">Provides services for get user data</param>
[ApiController]
[Route("recipes")]
[Authorize]
public class RecipesController(
    IRecipesRepository recipeService,
    IRecipeBooksRepository recipeBookService
) : ControllerBase
{
    private const string OrderIdIncreasing = "id-increasing";
    private const string OrderIdDecreasing = "id-decreasing";

    /// <summary>
    /// Gets a recipe by id
    /// <br />
    /// <pre><code>
    /// GET /recipes/{recipeKey}
    ///
    /// </code></pre>
    /// </summary>
    /// <param name="recipeKey">the recipe key from the request</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpGet("{recipeKey}")]
    public async Task<Results<Ok<RecipeJsonResponse>, ForbidHttpResult, NotFound>> GetRecipeById(
        [FromRoute] string recipeKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        RecipeDao? recipe = await recipeService.GetRecipeAsync(
            recipeKey,
            userKey,
            cancellationToken
        );
        if (recipe is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new RecipeJsonResponse(recipe));
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
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpDelete("{recipeKey}")]
    public async Task<
        Results<Ok, ForbidHttpResult, NoContent, NotFound, PreconditionFailedHttpResult>
    > DeleteRecipeById(
        [FromRoute] string recipeKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DeleteRecipeByIdResult deleteResult = await recipeService.DeleteRecipeAsync(
            recipeKey,
            userKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult switch
        {
            DeleteRecipeByIdResult.Success => TypedResults.NoContent(),
            DeleteRecipeByIdResult.NotFound => TypedResults.NotFound(),
            DeleteRecipeByIdResult.Forbidden => TypedResults.Forbid(),
            DeleteRecipeByIdResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
            _ => throw new NotImplementedException(),
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
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpPut("{recipeKey}")]
    public async Task<
        Results<
            NoContent,
            ForbidHttpResult,
            ValidationProblem,
            Ok<RecipeJsonResponse>,
            PreconditionFailedHttpResult,
            NotFound
        >
    > UpdateRecipeById(
        [FromRoute] string recipeKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromBody] RecipeJsonRequest body,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        UpdateRecipeResult updateResult = await recipeService.UpdateRecipeAsync(
            recipeKey,
            userKey,
            ifMatch.Value,
            new()
            {
                Name = body.Name,
                ShortDescription = body.ShortDescription,
                Details = body.Details,
            },
            cancellationToken
        );

        return updateResult switch
        {
            UpdateRecipeResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
            UpdateRecipeResult.NotFound => TypedResults.NotFound(),
            UpdateRecipeResult.Forbidden => TypedResults.Forbid(),
            UpdateRecipeResult.ValidationFailure validationError => TypedResults.ValidationProblem(
                validationError.Errors
            ),
            UpdateRecipeResult.Success success => TypedResults.Ok(
                new RecipeJsonResponse(success.Recipe)
            ),
            _ => throw new NotImplementedException(),
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
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the user closes the connection</param>
    /// <returns>Task resolving to the response to send back</returns>
    [HttpPost]
    public async Task<
        Results<NotFound, ValidationProblem, Created<RecipeJsonResponse>, ForbidHttpResult>
    > CreateRecipe(
        [FromBody] RecipeJsonRequest body,
        [BindRequired] [FromQuery(Name = "book")] string bookKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        CreateRecipeResult createRecipeResult = await recipeService.CreateRecipeAsync(
            bookKey,
            userKey,
            new()
            {
                Name = body.Name,
                ShortDescription = body.ShortDescription,
                Details = body.Details,
            },
            cancellationToken
        );

        return createRecipeResult switch
        {
            CreateRecipeResult.NotFound => TypedResults.NotFound(),
            CreateRecipeResult.Forbidden => TypedResults.Forbid(),
            CreateRecipeResult.ValidationFailure validation => TypedResults.ValidationProblem(
                validation.Errors
            ),
            CreateRecipeResult.Success success => TypedResults.Created(
                (string?)null,
                new RecipeJsonResponse(success.Recipe)
            ),
            _ => throw new NotImplementedException(),
        };
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
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cursor"></param>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<
        Results<
            ValidationProblem,
            NotFound,
            ForbidHttpResult,
            InternalServerError,
            Ok<IAsyncEnumerable<RecipeJsonResponse>>
        >
    > GetRecipes(
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken,
        [FromQuery(Name = "after-id")] string? afterId,
        [FromQuery(Name = "before-id")] string? beforeId,
        [FromQuery(Name = "result-ordering")] string? resultOrdering,
        [FromQuery(Name = "book")] string? bookId,
        [FromQuery(Name = "page-size")] int pageSize = 50
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (bookId is not null)
        {
            RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
                bookId,
                userKey,
                cancellationToken
            );

            if (book is null)
            {
                return TypedResults.NotFound();
            }
        }

        Database.RecordOrdering? recordOrdering = resultOrdering switch
        {
            OrderIdDecreasing => Database.RecordOrdering.ByIdDecreasing,
            null or OrderIdIncreasing => Database.RecordOrdering.ByIdIncreasing,
            _ => null,
        };

        if (recordOrdering is null)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "result-ordering", [$"value '{resultOrdering}' is not valid"] },
                }
            );
        }

        return TypedResults.Ok(
            recipeService
                .ListRecipesAsync(
                    userKey,
                    new()
                    {
                        RecipeBookId = bookId,
                        AfterRecipeId = afterId,
                        BeforeRecipeId = beforeId,
                        ResultOrder = recordOrdering.Value,
                        ResultCount = Math.Min(100, Math.Max(1, pageSize)),
                    },
                    cancellationToken
                )
                .Select(r => new RecipeJsonResponse(r))
        );
    }
}
