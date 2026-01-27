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
using Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;
using Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipe book
/// </summary>
/// <param name="recipeBookService">Provides services for manipulating recipe books</param>
/// <param name="userService">Provides services for getting user data</param>
[ApiController]
[Route("recipe-books")]
public class RecipeBooksController(IRecipeBookService recipeBookService, IUserService userService)
    : ControllerBase
{
    /// <summary>
    /// Gets a book by ID
    /// <pre><code>
    /// GET /recipe-books/{bookId}
    /// </code></pre>
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="user">The authenticated user</param>
    /// <param name="cancellationToken">token cancelled when the remote closes their connection</param>
    /// <returns>A HTTP response indicating the outcome of the operation</returns>
    [HttpGet("{bookId}")]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<
        Results<
            Ok<SingleRecipeBookResponseJson>,
            Created<SingleRecipeBookResponseJson>,
            ProblemHttpResult
        >
    > GetBookById(
        [RequiredAndNotEmpty(ErrorMessage = "A recipe book id must be supplied")] string bookId,
        User user,
        CancellationToken cancellationToken
    )
    {
        RecipeBookDao? book = await recipeBookService.GetRecipeBookAsync(
            bookId,
            user.UserId,
            cancellationToken
        );

        if (book is null)
        {
            return BookNotFoundResult(bookId);
        }

        return await SendRecipeBookResponseAsync(book, cancellationToken);
    }

    /// <summary>
    /// Updates a recipe book <br />
    /// <pre><code>
    /// PUT /recipe-books/{bookId}
    /// If-Match: {ifMatch}
    ///
    /// {
    ///     "name": "... recipe book name ...",
    ///     "shortDescription": " ... recipe book short description ... "
    /// }
    /// </code></pre>
    /// </summary>
    /// <param name="bookId">The id of the recipe book</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="user">Information about the authenticated user</param>
    /// <param name="requestBody">Incoming data in the body</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpPut("{bookId}")]
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
        [RequiredAndNotEmpty(ErrorMessage = "A recipe book id must be supplied")] string bookId,
        [RequiredAndValidIfMatch] [FromHeader(Name = "if-match")] string ifMatch,
        User user,
        [FromBody] RecipeBookJsonBody requestBody,
        CancellationToken cancellationToken
    )
    {
        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        var updateResult = await recipeBookService.UpdateRecipeBookDetailsAsync(
            bookId,
            user.UserId,
            new()
            {
                Name = requestBody.Name,
                ShortDescription = requestBody.ShortDescription,
                ConcurrencyTag = ifMatch,
            },
            cancellationToken
        );

        if (updateResult is UpdateRecipeBookDetailsSuccess success)
        {
            return await SendRecipeBookResponseAsync(success.RecipeBook, cancellationToken);
        }

        if (updateResult is not UpdateRecipeBookDetailsFailure failure)
        {
            return InternalServerError(bookId);
        }

        return failure.Reason switch
        {
            UpdateRecipeBookDetailsFailureReason.NotFound => BookNotFoundResult(bookId),
            UpdateRecipeBookDetailsFailureReason.DoesNotHaveAccess => OperationOnBookForbidden(
                bookId
            ),
            UpdateRecipeBookDetailsFailureReason.ConcurrencyConflict => BookPreconditionFailed(
                bookId,
                "If-Match"
            ),
            UpdateRecipeBookDetailsFailureReason.ValidationFailure => BookValidationProblem(
                bookId,
                failure.Errors
            ),
            _ => InternalServerError(bookId),
        };
    }

    /// <summary>
    /// Deletes a recipe book <br />
    /// <pre><code>
    /// DELETE /recipe-books/{bookId}
    /// If-Match: {ifMatch}
    /// </code></pre>
    /// </summary>
    /// <param name="bookId">The id of the recipe book</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="user">Information about the authenticated user</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpDelete("{bookId}")]
    public async Task<Results<NoContent, ProblemHttpResult>> DeleteRecipeBookById(
        [RequiredAndNotEmpty(ErrorMessage = "A recipe book id must be supplied")] string bookId,
        [RequiredAndNotEmpty(ErrorMessage = "An If-Match header must be supplied")]
        [FromHeader(Name = "if-match")]
            string ifMatch,
        User user,
        CancellationToken cancellationToken
    )
    {
        DeleteRecipeBookResult deleteResult = await recipeBookService.DeleteRecipeBookAsync(
            bookId,
            user.UserId,
            ifMatch,
            cancellationToken
        );

        return deleteResult.Outcome switch
        {
            DeleteRecipeBookResultOutcome.Success => TypedResults.NoContent(),
            DeleteRecipeBookResultOutcome.NotFound => BookNotFoundResult(bookId),
            DeleteRecipeBookResultOutcome.DoesNotHaveAccess => OperationOnBookForbidden(bookId),
            DeleteRecipeBookResultOutcome.ConcurrencyConflict => BookPreconditionFailed(
                bookId,
                "If-Match"
            ),
            _ => InternalServerError(bookId),
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
    /// <param name="user">Information about the authenticated user</param>
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
        User user,
        [FromBody] RecipeBookJsonBody requestBody,
        CancellationToken cancellationToken
    )
    {
        // The service layer will do all of our validation for us. We can just call it and then transform its
        // result back into the correct HTTP code and response.
        var createResult = await recipeBookService.CreateRecipeBookAsync(
            user.UserId,
            new() { Name = requestBody.Name, ShortDescription = requestBody.ShortDescription },
            cancellationToken
        );

        return createResult.Outcome switch
        {
            CreateRecipeBookResultOutcome.Success => await SendRecipeBookResponseAsync(
                createResult.Book,
                cancellationToken
            ),
            CreateRecipeBookResultOutcome.PermissionFailure =>
                CustomProblemHttpResults.OperationForbidden(),
            CreateRecipeBookResultOutcome.ValidationErrors => BookCreateValidationProblem(
                createResult.ValidationErrors
            ),
            _ => CustomProblemHttpResults.InternalServerError(),
        };
    }

    /// <summary>
    /// Gets the user's books
    /// <br />
    /// <br />
    /// Path to get previous or next page:
    /// <pre><code>
    /// GET /recipe-books?page-id={bookId}&amp;going={forward | backward}
    /// </code></pre>
    /// <br />
    /// Path to get first page:
    /// <pre><code>
    /// GET /recipe-books
    /// </code></pre>
    /// </summary>
    /// <param name="user">Information about the authenticated user</param>
    /// <param name="cursor">The page cursor</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the HTTP response</returns>
    [HttpGet]
    [ResponseCache(Duration = 15 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<
        Results<ValidationProblem, ProblemHttpResult, Ok<RecipeBookPageResponseJson>>
    > GetBooks(User user, PageCursor cursor, CancellationToken cancellationToken)
    {
        var pageIterator = await recipeBookService.ListRecipeBooksAsync(
            user.UserId,
            new()
            {
                AfterBookId =
                    cursor.Going?.Direction == NavigationDirection.DirectionValue.Forwards
                        ? cursor.Index
                        : null,
                BeforeBookId =
                    cursor.Going?.Direction == NavigationDirection.DirectionValue.Backwards
                        ? cursor.Index
                        : null,
                ResultOrder =
                    cursor.Going?.Direction == NavigationDirection.DirectionValue.Backwards
                        ? ListRecipeBooksOrdering.ByIdDecreasing
                        : ListRecipeBooksOrdering.ByIdIncreasing,
                ResultCount = 10,
            },
            cancellationToken
        );

        if (pageIterator is null)
        {
            return CustomProblemHttpResults.InternalServerError();
        }

        List<RecipeBookDao> pageData = await pageIterator
            .OrderBy(book => book.Id)
            .ToListAsync(cancellationToken);

        bool? hasNextPage;
        string? idForNextPage = cursor.ComputeIdForNextPage(r => r.Id, pageData);
        if (cursor.ShouldRunNextPageQuery(idForNextPage))
        {
            hasNextPage = await HasResultsBeyondPageQuery(
                idForNextPage,
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
            new RecipeBookPageResponseJson()
            {
                RecipeBooks = [.. pageData.Select(b => new RecipeBookJson(b))],
                Users = await CommonQueries
                    .FetchUsersAsync(
                        pageData.GroupBy(book => book.OwnerUserId).Select(g => g.Key),
                        userService,
                        cancellationToken
                    )
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
    private async Task<bool?> HasResultsBeyondPageQuery(
        string? bookId,
        bool isBack,
        User user,
        CancellationToken cancellationToken
    )
    {
        var pageIterator = await recipeBookService.ListRecipeBooksAsync(
            user.UserId,
            new()
            {
                AfterBookId = isBack ? null : bookId,
                BeforeBookId = isBack ? bookId : null,
                ResultCount = 1,
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
    /// Returns a validation problem scoped to a book
    /// </summary>
    /// <param name="bookId">The book ID</param>
    /// <param name="errors">Map of validation errors</param>
    /// <returns>The validation problem</returns>
    private static ValidationProblem BookValidationProblem(
        string bookId,
        IDictionary<string, string[]>? errors
    )
    {
        return TypedResults.ValidationProblem(
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            title: "Validation Problem",
            detail: $"Requested operation against recipe book \"{bookId}\" failed because one or more validation errors",
            errors: errors ?? new Dictionary<string, string[]>(),
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }

    /// <summary>
    /// Returns a validation problem scoped to a book creation request
    /// </summary>
    /// <param name="errors">Map of validation errors</param>
    /// <returns>The validation problem</returns>
    private static ValidationProblem BookCreateValidationProblem(
        IDictionary<string, string[]> errors
    )
    {
        return TypedResults.ValidationProblem(
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            title: "Validation Problem",
            detail: "Creating recipe book failed because of one or more validation errors",
            errors: errors
        );
    }

    /// <summary>
    /// Generates a 412 pre-condition failure
    /// </summary>
    /// <param name="bookId">the recipe book id</param>
    /// <param name="failedHeader">The header that triggered the failure</param>
    /// <returns>the 412 result</returns>
    private static ProblemHttpResult BookPreconditionFailed(string bookId, string failedHeader)
    {
        return TypedResults.Problem(
            statusCode: 412,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-412-precondition-failed",
            title: "Pre-Condition Failure",
            detail: $"Requested operation against recipe book \"{bookId}\" failed because one or more conditions in the request headers could not be satisfied.",
            extensions:
            [
                new KeyValuePair<string, object?>("bookId", bookId),
                new KeyValuePair<string, object?>("failedHeader", failedHeader),
            ]
        );
    }

    /// <summary>
    /// Generates a 403 forbidden error
    /// </summary>
    /// <param name="bookId">the recipe book id</param>
    /// <returns>the 404 result</returns>
    public static ProblemHttpResult OperationOnBookForbidden(string bookId)
    {
        return TypedResults.Problem(
            statusCode: 403,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
            title: "Operation on Recipe Book Forbidden",
            detail: $"Caller lacks required permissions to perform requested action on recipe book \"{bookId}\".",
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }

    /// <summary>
    /// Generates a 404 error
    /// </summary>
    /// <param name="bookId">the recipe book id</param>
    /// <returns>the 404 result</returns>
    public static ProblemHttpResult BookNotFoundResult(string bookId)
    {
        return TypedResults.Problem(
            statusCode: 404,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            title: "Recipe Book Not Found",
            detail: $"Recipe book \"{bookId}\" was either not found or caller does not have access.",
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }

    /// <summary>
    /// Generates an internal server error
    /// </summary>
    /// <param name="bookId">the recipe book id</param>
    /// <returns>the 500 error</returns>
    private static ProblemHttpResult InternalServerError(string bookId)
    {
        return TypedResults.Problem(
            statusCode: 500,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error",
            title: "Server Error Retrieving Recipe Book",
            detail: $"The server was unable to full fill the request for recipe book \"{bookId}\" because something went wrong.",
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
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
        IUserDao? owningUser = await userService.GetUserAsync(book.OwnerUserId, cancellationToken);
        if (owningUser is null)
        {
            return InternalServerError(book.Id);
        }

        var jsonData = new SingleRecipeBookResponseJson()
        {
            RecipeBook = new(book),
            Owner = new(owningUser),
        };

        if (isCreate)
        {
            return TypedResults.Created((string?)null, jsonData);
        }
        else
        {
            Response.Headers.ETag = book.ConcurrencyTag;
            Response.Headers.LastModified = book.LastUpdated.ToString("R");
            return TypedResults.Ok(jsonData);
        }
    }
}
