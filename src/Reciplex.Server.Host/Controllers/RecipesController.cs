using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipes
/// </summary>
/// <param name="recipeService">Provides services for getting recipe data</param>
[ApiController]
[Route("recipes")]
[Authorize]
public class RecipesController(IRecipesService recipeService) : ControllerBase
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeService.GetRecipeAsync(recipeKey, userKey, cancellationToken);

        return result.Result switch
        {
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            UserNotFoundResult => TypedResults.Forbid(),
            SuccessResult<RecipeDao> success => TypedResults.Ok(
                new RecipeJsonResponse(success.Value)
            ),
            _ => throw new NotImplementedException(),
        };
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            EmptySuccessResult,
            Database.Results.NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            Database.Results.ConflictResult
        > deleteResult = await recipeService.DeleteRecipeAsync(
            recipeKey,
            userKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult.Result switch
        {
            EmptySuccessResult => TypedResults.NoContent(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ForbiddenResult or UserNotFoundResult => TypedResults.Forbid(),
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            Database.Results.NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > updateResult = await recipeService.UpdateRecipeAsync(
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

        return updateResult.Result switch
        {
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ForbiddenResult or UserNotFoundResult => TypedResults.Forbid(),
            ValidationFailureResult validationError => TypedResults.ValidationProblem(
                validationError.Errors
            ),
            SuccessResult<RecipeDao> success => TypedResults.Ok(
                new RecipeJsonResponse(success.Value)
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            Database.Results.NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        > createRecipeResult = await recipeService.CreateRecipeAsync(
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

        return createRecipeResult.Result switch
        {
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ForbiddenResult or UserNotFoundResult => TypedResults.Forbid(),
            ValidationFailureResult validation => TypedResults.ValidationProblem(validation.Errors),
            SuccessResult<RecipeDao> success => TypedResults.Created(
                (string?)null,
                new RecipeJsonResponse(success.Value)
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1068:CancellationToken parameters must come last",
        Justification = "This is a ASP.NET controller method"
    )]
    public async Task<
        Results<
            ValidationProblem,
            NotFound,
            ForbidHttpResult,
            InternalServerError,
            Ok<IEnumerable<RecipeListEntryJsonResponse>>
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
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

        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            Database.Results.NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        > result = await recipeService.ListRecipesAsync(
            userKey,
            new()
            {
                RecipeBookKey = bookId,
                AfterRecipeKey = afterId,
                BeforeRecipeKey = beforeId,
                ResultOrder = recordOrdering.Value,
                ResultCount = Math.Min(100, Math.Max(1, pageSize)),
            },
            cancellationToken
        );

        return result.Result switch
        {
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            UserNotFoundResult => TypedResults.Forbid(),
            ValidationFailureResult validationFailureResult => TypedResults.ValidationProblem(
                validationFailureResult.Errors
            ),
            SuccessResult<List<RecipeListEntryDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(r => new RecipeListEntryJsonResponse(r))
            ),
            _ => throw new NotImplementedException(),
        };
    }
}
