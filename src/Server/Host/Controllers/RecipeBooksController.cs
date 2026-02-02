using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime.Text;
using Reciplex.Server.Host.Models.HttpPrimitives;
using Reciplex.Server.Host.Models.PagingUtils;
using Reciplex.Server.Host.Models.RecipeBook;
using Reciplex.Server.Host.Models.User;
using Reciplex.Server.Host.Utils;
using Reciplex.Server.Host.Utils.HttpResults;
using Reciplex.Server.Host.Utils.UserKeyUtils;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipe book
/// </summary>
/// <param name="recipeBookService">Provides services for manipulating recipe books</param>
/// <param name="userService">Provides services for getting user data</param>
/// <param name="bookProblemFactory">factory for creating recipe book problem results</param>
[ApiController]
[Route("recipe-books")]
[TypeFilter(typeof(InternalServerErrorOnException))]
public class RecipeBooksController(
    IRecipeBookService recipeBookService,
    IUserService userService,
    IRecipeBookProblemFactory bookProblemFactory
) : ControllerBase
{
    /// <summary>
    /// Gets a book by ID
    /// <pre><code>
    /// GET /recipe-books/{bookKey}
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The key of the book</param>
    /// <param name="appClaimsPrincipal">The authenticated user</param>
    /// <param name="cancellationToken">token cancelled when the remote closes their connection</param>
    /// <returns>A HTTP response indicating the outcome of the operation</returns>
    [HttpGet("{bookKey}")]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<
        Results<
            Ok<SingleRecipeBookResponseJson>,
            Created<SingleRecipeBookResponseJson>,
            ProblemHttpResult
        >
    > GetBookById(
        [FromRoute] [BindRequired] RecipeBookKey bookKey,
        [UseModelBinderProvider] ApplicationClaimsPrincipal appClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
            bookKey,
            appClaimsPrincipal.UserKey,
            cancellationToken
        );

        if (book is null)
        {
            return bookProblemFactory.BookNotFoundResult(bookKey);
        }

        return await SendRecipeBookResponseAsync(book, cancellationToken);
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
    /// <param name="appClaimsPrincipal">Information about the authenticated user</param>
    /// <param name="requestBody">Incoming data in the body</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpPut("{bookKey}")]
    public async Task<
        Results<
            Results<
                Ok<SingleRecipeBookResponseJson>,
                Created<SingleRecipeBookResponseJson>,
                ProblemHttpResult
            >,
            ProblemHttpResult,
            ValidationProblem
        >
    > UpdateBookById(
        [FromRoute] [BindRequired] RecipeBookKey bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [UseModelBinderProvider] ApplicationClaimsPrincipal appClaimsPrincipal,
        [FromBody] RecipeBookJsonBody requestBody,
        CancellationToken cancellationToken
    )
    {
        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        var updateResult = await recipeBookService.UpdateRecipeBookDetailsAsync(
            bookKey,
            appClaimsPrincipal.UserKey,
            new()
            {
                Name = requestBody.Name,
                ShortDescription = requestBody.ShortDescription,
                ConcurrencyTag = ifMatch.Value,
            },
            cancellationToken
        );

        return updateResult.Outcome switch
        {
            UpdateRecipeBookDetailsOutcome.Success => await SendRecipeBookResponseAsync(
                updateResult.RecipeBook,
                cancellationToken
            ),
            UpdateRecipeBookDetailsOutcome.NotFound => bookProblemFactory.BookNotFoundResult(
                bookKey
            ),
            UpdateRecipeBookDetailsOutcome.LacksPermission =>
                bookProblemFactory.OperationOnBookForbidden(bookKey),
            UpdateRecipeBookDetailsOutcome.ConcurrencyConflict =>
                bookProblemFactory.BookPreconditionFailed(bookKey, "If-Match"),
            UpdateRecipeBookDetailsOutcome.ValidationFailure =>
                bookProblemFactory.BookValidationProblem(bookKey, updateResult.Errors),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(UpdateRecipeBookDetailsOutcome)} branch {updateResult.Outcome}"
            ),
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
    /// <param name="appClaimsPrincipal">Information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpDelete("{bookKey}")]
    public async Task<Results<NoContent, ProblemHttpResult>> DeleteRecipeBookById(
        [FromRoute] [BindRequired] RecipeBookKey bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [UseModelBinderProvider] ApplicationClaimsPrincipal appClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        DeleteRecipeBookResult deleteResult = await recipeBookService.DeleteRecipeBookAsync(
            bookKey,
            appClaimsPrincipal.UserKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult.Outcome switch
        {
            DeleteRecipeBookResultOutcome.Success => TypedResults.NoContent(),
            DeleteRecipeBookResultOutcome.NotFound => bookProblemFactory.BookNotFoundResult(
                bookKey
            ),
            DeleteRecipeBookResultOutcome.LacksPermission =>
                bookProblemFactory.OperationOnBookForbidden(bookKey),
            DeleteRecipeBookResultOutcome.ConcurrencyConflict =>
                bookProblemFactory.BookPreconditionFailed(bookKey, "If-Match"),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(DeleteRecipeBookResultOutcome)} branch {deleteResult.Outcome}"
            ),
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
    /// <param name="appClaimsPrincipal">Information about the authenticated user</param>
    /// <param name="requestBody">Incoming data in the body</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpPost]
    public async Task<
        Results<
            Results<
                Ok<SingleRecipeBookResponseJson>,
                Created<SingleRecipeBookResponseJson>,
                ProblemHttpResult
            >,
            ProblemHttpResult,
            ValidationProblem
        >
    > CreateBook(
        [UseModelBinderProvider] ApplicationClaimsPrincipal appClaimsPrincipal,
        [FromBody] RecipeBookJsonBody requestBody,
        CancellationToken cancellationToken
    )
    {
        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        var createResult = await recipeBookService.CreateRecipeBookAsync(
            appClaimsPrincipal.UserKey,
            new() { Name = requestBody.Name, ShortDescription = requestBody.ShortDescription },
            cancellationToken
        );

        return createResult.Outcome switch
        {
            CreateRecipeBookResultOutcome.Success => await SendRecipeBookResponseAsync(
                createResult.Book,
                cancellationToken
            ),
            CreateRecipeBookResultOutcome.ValidationErrors =>
                bookProblemFactory.BookValidationProblem(null, createResult.ValidationErrors),
            _ => throw new NotImplementedException(
                $"unhandled {nameof(CreateRecipeBookResultOutcome)} branch {createResult.Outcome}"
            ),
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
    /// <param name="appClaimsPrincipal">Information about the authenticated user</param>
    /// <param name="cursor">The page cursor</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the HTTP response</returns>
    [HttpGet]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<
        Results<ValidationProblem, ProblemHttpResult, Ok<RecipeBookPageResponseJson>>
    > GetBooks(
        [UseModelBinderProvider] ApplicationClaimsPrincipal appClaimsPrincipal,
        [FromQuery] PageCursor<RecipeBookKey?> cursor,
        [FromQuery(Name = "page-size")] int? pageSize,
        CancellationToken cancellationToken
    )
    {
        pageSize = Math.Min(20, Math.Max(1, pageSize ?? 10));

        IAsyncEnumerable<RecipeBookDao> pageIterator = await recipeBookService.ListRecipeBooksAsync(
            appClaimsPrincipal.UserKey,
            new()
            {
                AfterBookId =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Forwards
                        ? cursor.Index
                        : null,
                BeforeBookId =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Backwards
                        ? cursor.Index
                        : null,
                ResultOrder =
                    cursor.GoingDirection == NavigationDirection.DirectionValue.Backwards
                        ? ListRecipeBooksOrdering.ByIdDecreasing
                        : ListRecipeBooksOrdering.ByIdIncreasing,
                ResultCount = pageSize.Value,
            },
            cancellationToken
        );

        List<RecipeBookJson> pageData = await pageIterator
            .Select(b => new RecipeBookJson(b))
            .OrderBy(book => book.BookKey.SurrogateKey)
            .ToListAsync(cancellationToken);

        (bool hasNextPage, RecipeBookKey? idForNextPage) = await cursor.QueryForNextPage(
            r => r.BookKey,
            pageData,
            (id, cancellationToken) =>
                HasResultsBeyondPageQuery(id, false, appClaimsPrincipal.UserKey, cancellationToken),
            cancellationToken
        );

        (bool hasPreviousPage, RecipeBookKey? idForPreviousPage) =
            await cursor.QueryForPreviousPage(
                r => r.BookKey,
                pageData,
                (id, cancellationToken) =>
                    HasResultsBeyondPageQuery(
                        id,
                        true,
                        appClaimsPrincipal.UserKey,
                        cancellationToken
                    ),
                cancellationToken
            );

        return TypedResults.Ok(
            new RecipeBookPageResponseJson()
            {
                RecipeBooks = pageData,
                Users = await userService
                    .FetchUsersAsync(
                        pageData.GroupBy(book => book.OwningUserKey).Select(g => g.Key),
                        cancellationToken
                    )
                    .Select(u => new UserJson(u))
                    .ToListAsync(cancellationToken),
                NextPage =
                    hasNextPage && idForNextPage != null
                        ? new()
                        {
                            GoingQueryParam = new NavigationDirection()
                            {
                                Direction = NavigationDirection.DirectionValue.Forwards,
                            },
                            PageIndex = idForNextPage.Value,
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
                        }
                        : null,
            }
        );
    }

    /// <summary>
    /// Runs a query to check if there are results outside the bounds of the page.
    /// <br />
    /// If <paramref name="isBack"/> is false then this will return true if <c>ANY BOOK.ID GREATER THAN bookId</c>.
    /// <br />
    /// If <paramref name="isBack"/> is true then this will return true if <c>ANY BOOK.ID LESS THAN bookId</c>.
    /// </summary>
    /// <param name="bookId">The page boundary</param>
    /// <param name="isBack">True to check if there are results before a page <c>
    /// WHERE BOOK.ID LESS THEN bookId
    /// </c></param>
    /// <param name="cancellationToken">Cancels the operation</param>
    /// <returns>Task that resolves to the result of the query or null if the query fails</returns>
    private async Task<bool> HasResultsBeyondPageQuery(
        RecipeBookKey? bookId,
        bool isBack,
        UserKey userKey,
        CancellationToken cancellationToken
    )
    {
        var pageIterator = await recipeBookService.ListRecipeBooksAsync(
            userKey,
            new()
            {
                AfterBookId = isBack ? null : bookId,
                BeforeBookId = isBack ? bookId : null,
                ResultCount = 1,
            },
            cancellationToken
        );

        await foreach (var _ in pageIterator)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sends information about a recipe book back to the caller
    /// </summary>
    /// <param name="book">the book send to back</param>
    /// <param name="cancellationToken">token that cancels when the remote closes the connection</param>
    /// <returns>The http outcome including possible failure codes in the event the recipe book could not be sent back</returns>
    private async Task<
        Results<
            Ok<SingleRecipeBookResponseJson>,
            Created<SingleRecipeBookResponseJson>,
            ProblemHttpResult
        >
    > SendRecipeBookResponseAsync(
        RecipeBookDao book,
        CancellationToken cancellationToken,
        bool isCreate = false
    )
    {
        // All recipe books should have an owner. If this one lacks one, then we can't full fill
        // our API contracts so return 500 error.
        UserDao? owningUser =
            await userService.GetUserAsync(book.OwningUserKey, cancellationToken)
            ?? throw new InvalidOperationException(
                $"expects {book.OwningUserKey} to exist for book {book.Id}"
            );
        var jsonData = new SingleRecipeBookResponseJson()
        {
            RecipeBook = new(book),
            RecipeBookOwner = new(owningUser),
        };

        if (isCreate)
        {
            return TypedResults.Created((string?)null, jsonData);
        }
        else
        {
            return TypedResults.Ok(jsonData);
        }
    }
}
