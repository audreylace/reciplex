using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipes
/// </summary>
/// <param name="recipeService">Provides services for getting recipe data</param>
/// <param name="stringIdProvider">service for marshalling ids to and from strings</param>
[ApiController]
[Route("recipes")]
[Authorize]
public class RecipesController(IRecipesService recipeService, IStringIdProvider stringIdProvider)
    : ControllerBase
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
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (stringIdProvider.AsLong(recipeKey) is not long recipeId || recipeId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeService.GetRecipeAsync(recipeId, userId, cancellationToken);

        return result.Result switch
        {
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            UserNotFoundResult => TypedResults.Forbid(),
            SuccessResult<RecipeDao> success => TypedResults.Ok(ToJson(success.Value)),
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<Ok, ForbidHttpResult, NoContent, NotFound, PreconditionFailedHttpResult>
    > DeleteRecipeById(
        [FromRoute] string recipeKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (stringIdProvider.AsLong(recipeKey) is not long recipeId || recipeId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            EmptySuccessResult,
            Database.Results.NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            Database.Results.ConflictResult
        > deleteResult = await recipeService.QueueRecipeDeleteAsync(
            recipeId,
            userId,
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<
            NoContent,
            ForbidHttpResult,
            BadRequest,
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
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (stringIdProvider.AsLong(recipeKey) is not long recipeId || recipeId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            Database.Results.NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > updateResult = await recipeService.UpdateRecipeAsync(
            recipeId,
            userId,
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
            ValidationFailureResult => TypedResults.BadRequest(),
            SuccessResult<RecipeDao> success => TypedResults.Ok(ToJson(success.Value)),
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<NotFound, BadRequest, Created<RecipeJsonResponse>, ForbidHttpResult>
    > CreateRecipe(
        [FromBody] RecipeJsonRequest body,
        [BindRequired] [FromQuery(Name = "book")] string bookKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            Database.Results.NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        > createRecipeResult = await recipeService.CreateRecipeAsync(
            bookId,
            userId,
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
            ValidationFailureResult => TypedResults.BadRequest(),
            SuccessResult<RecipeDao> success => TypedResults.Created(
                (string?)null,
                ToJson(success.Value)
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
    [HttpGet]
    public async Task<
        Results<
            BadRequest,
            NotFound,
            ForbidHttpResult,
            InternalServerError,
            Ok<IEnumerable<RecipeListEntryJsonResponse>>
        >
    > GetRecipes(
        [FromQuery(Name = "user")] string userKey,
        [FromQuery(Name = "after-id")] string? afterIdString,
        [FromQuery(Name = "before-id")] string? beforeIdString,
        [FromQuery(Name = "result-ordering")] string? resultOrdering,
        [FromQuery(Name = "book")] string? bookIdString,
        CancellationToken cancellationToken,
        [FromQuery(Name = "page-size")] int pageSize = 50
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        long? afterId = null;
        if (!string.IsNullOrWhiteSpace(afterIdString))
        {
            afterId = stringIdProvider.AsLong(afterIdString);
            if (afterId is null || afterId < 0)
            {
                return TypedResults.BadRequest();
            }
        }

        long? beforeId = null;
        if (!string.IsNullOrWhiteSpace(beforeIdString))
        {
            beforeId = stringIdProvider.AsLong(beforeIdString);
            if (beforeId is null || beforeId < 0)
            {
                return TypedResults.BadRequest();
            }
        }

        long? bookId = null;
        if (!string.IsNullOrWhiteSpace(bookIdString))
        {
            bookId = stringIdProvider.AsLong(bookIdString);
            if (bookId is null || bookId < 0)
            {
                return TypedResults.BadRequest();
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
            return TypedResults.BadRequest();
        }

        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeService.ListRecipesAsync(
            userId,
            new()
            {
                RecipeBookId = bookId,
                AfterRecipeId = afterId,
                BeforeRecipeId = beforeId,
                ResultOrder = recordOrdering.Value,
                ResultCount = Math.Min(100, Math.Max(1, pageSize)),
            },
            cancellationToken
        );

        return result.Result switch
        {
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            UserNotFoundResult => TypedResults.Forbid(),
            SuccessResult<List<RecipeListEntryDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(ToListEntryJson)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a recipe json model from a <see cref="RecipeListEntryDao"/>
    /// </summary>
    /// <param name="recipe">the source recipe data used to populate this model</param>
    public RecipeListEntryJsonResponse ToListEntryJson(RecipeListEntryDao recipe)
    {
        return new RecipeListEntryJsonResponse()
        {
            RecipeKey = stringIdProvider.AsString(recipe.Id),
            Name = recipe.Name,
            ShortDescription = recipe.ShortDescription,
            BookKey = stringIdProvider.AsString(recipe.BookId),
        };
    }

    /// <summary>
    /// Creates a recipe json model from a <see cref="RecipeDao"/>
    /// </summary>
    /// <param name="recipe">the source recipe data used to populate <see cref="RecipeJsonResponse"/> </param>
    public RecipeJsonResponse ToJson(RecipeDao recipe)
    {
        return new RecipeJsonResponse()
        {
            RecipeKey = stringIdProvider.AsString(recipe.Id),
            Name = recipe.Name,
            ShortDescription = recipe.ShortDescription,
            Details = recipe.Details,
            BookKey = stringIdProvider.AsString(recipe.BookId),
            Created = recipe.Created,
            LastModified = recipe.LastModified,
            ConcurrencyTag = recipe.ConcurrencyTag,
            MayEdit = recipe.MayEdit,
        };
    }
}
