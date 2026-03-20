using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipe book
/// </summary>
/// <param name="recipeBookService">Provides services for manipulating recipe books</param>
/// <param name="userService">Provides services for getting user data</param>
[ApiController]
[Route("recipe-books")]
[Authorize]
public class RecipeBooksController(IRecipeBooksRepository recipeBookService) : ControllerBase
{
    private const string OrderIdIncreasing = "id-increasing";
    private const string OrderIdDecreasing = "id-decreasing";

    /// <summary>
    /// Gets a book by ID
    /// <pre><code>
    /// GET /recipe-books/{bookKey}
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The key of the book</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token cancelled when the remote closes their connection</param>
    /// <returns>A HTTP response indicating the outcome of the operation</returns>
    [HttpGet("{bookKey}")]
    public async Task<
        Results<Ok<RecipeBookJsonResponse>, NotFound, ForbidHttpResult, InternalServerError>
    > GetBookById(
        [FromRoute] string bookKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
            bookKey,
            userKey,
            cancellationToken
        );

        if (book is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new RecipeBookJsonResponse(book));
    }

    /// <summary>
    /// Updates a recipe book <br />
    /// <pre><code>
    /// PUT /recipe-books/{bookKey}
    /// If-Match: {ifMatch}
    ///
    /// {
    ///     "name": "... recipe book name ...",
    ///     "shortDescription": " ... recipe book short description ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The key of the recipe book</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="requestBody">Incoming data in the body</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpPut("{bookKey}")]
    public async Task<
        Results<
            ValidationProblem,
            Ok<RecipeBookJsonResponse>,
            ForbidHttpResult,
            NotFound,
            PreconditionFailedHttpResult
        >
    > UpdateBookById(
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        [FromBody] RecipeBookJsonRequest requestBody,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        UpdateRecipeBookResult updateResult = await recipeBookService.UpdateRecipeBookAsync(
            bookKey,
            userKey,
            new()
            {
                Name = requestBody.Name,
                ShortDescription = requestBody.ShortDescription,
                ConcurrencyTag = ifMatch.Value,
            },
            cancellationToken
        );

        return updateResult switch
        {
            UpdateRecipeBookResult.Success success => TypedResults.Ok(
                new RecipeBookJsonResponse(success.RecipeBook)
            ),
            UpdateRecipeBookResult.NotFound => TypedResults.NotFound(),
            UpdateRecipeBookResult.Forbidden => TypedResults.Forbid(),
            UpdateRecipeBookResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
            UpdateRecipeBookResult.ValidationFailure failure => TypedResults.ValidationProblem(
                failure.Errors
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Deletes a recipe book <br />
    /// <pre><code>
    /// DELETE /recipe-books/{bookKey}
    /// If-Match: {ifMatch}
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The id of the recipe book</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpDelete("{bookKey}")]
    public async Task<
        Results<NoContent, PreconditionFailedHttpResult, ForbidHttpResult, NotFound>
    > DeleteRecipeBookById(
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }
        DeleteRecipeBookResult deleteResult = await recipeBookService.DeleteRecipeBookAsync(
            bookKey,
            userKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult switch
        {
            DeleteRecipeBookResult.Success => TypedResults.NoContent(),
            DeleteRecipeBookResult.NotFound => TypedResults.NotFound(),
            DeleteRecipeBookResult.Forbidden => TypedResults.Forbid(),
            DeleteRecipeBookResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a recipe book <br />
    /// <pre><code>
    /// POST /recipe-books
    ///
    /// {
    ///     "name": "... recipe book name ...",
    ///     "shortDescription": " ... recipe book short description ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="requestBody">Incoming data in the body</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpPost]
    public async Task<
        Results<ForbidHttpResult, Created<RecipeBookJsonResponse>, ValidationProblem>
    > CreateBook(
        [FromQuery(Name = "user")] [BindRequired] string userKey,
        [FromBody] RecipeBookJsonRequest requestBody,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        CreateRecipeBookResult createResult = await recipeBookService.CreateRecipeBookAsync(
            userKey,
            new() { Name = requestBody.Name, ShortDescription = requestBody.ShortDescription },
            cancellationToken
        );

        return createResult switch
        {
            CreateRecipeBookResult.Success success => TypedResults.Created(
                (string?)null,
                new RecipeBookJsonResponse(success.RecipeBook)
            ),
            CreateRecipeBookResult.ValidationFailure validationError =>
                TypedResults.ValidationProblem(validationError.Errors),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Gets the user's books
    /// <br />
    /// <br />
    /// Path to get previous or next page:
    /// <pre><code>
    /// GET /recipe-books?index={bookId}&amp;going={forward | backward}
    /// </code></pre>
    /// <br />
    /// Path to get first page:
    /// <pre><code>
    /// GET /recipe-books
    /// </code></pre>
    /// </summary>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cursor">The page cursor</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the HTTP response</returns>
    [HttpGet]
    public async Task<
        Results<ValidationProblem, ForbidHttpResult, Ok<IAsyncEnumerable<RecipeBookJsonResponse>>>
    > GetBooks(
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken,
        [FromQuery(Name = "after-id")] string? afterId,
        [FromQuery(Name = "before-id")] string? beforeId,
        [FromQuery(Name = "result-ordering")] string? resultOrdering,
        [FromQuery(Name = "page-size")] int pageSize = 50
    )
    {
        if (!await HttpContext.IsUser(userKey, cancellationToken))
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

        return TypedResults.Ok(
            recipeBookService
                .ListRecipeBooksAsync(
                    userKey,
                    new()
                    {
                        AfterBookId = afterId,
                        BeforeBookId = beforeId,
                        ResultOrder = recordOrdering.Value,
                        ResultCount = Math.Min(100, Math.Max(1, pageSize)),
                    },
                    cancellationToken
                )
                .Select(r => new RecipeBookJsonResponse(r))
        );
    }
}
