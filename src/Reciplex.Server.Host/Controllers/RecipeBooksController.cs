using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
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
public class RecipeBooksController(IRecipeBooksService recipeBookService) : ControllerBase
{
    private const string OrderIdIncreasing = "id-increasing";
    private const string OrderIdDecreasing = "id-decreasing";

    #region CRUD Endpoints for Recipe Book

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
        Results<Ok<RecipeBookJsonResponse>, NotFound, ForbidHttpResult, ValidationProblem>
    > GetBookById(
        [FromRoute] string bookKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookKey, userKey, cancellationToken);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ValidationFailureResult error => TypedResults.ValidationProblem(error.Errors),
            SuccessResult<RecipeBookDao> { Value: RecipeBookDao book } => TypedResults.Ok(
                new RecipeBookJsonResponse(book)
            ),
            _ => throw new NotImplementedException(),
        };
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > updateResult = await recipeBookService.UpdateRecipeBookAsync(
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

        return updateResult.Result switch
        {
            SuccessResult<RecipeBookDao> success => TypedResults.Ok(
                new RecipeBookJsonResponse(success.Value)
            ),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> => TypedResults.Forbid(),
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
            ValidationFailureResult failure => TypedResults.ValidationProblem(failure.Errors),
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
        Results<Ok, PreconditionFailedHttpResult, ForbidHttpResult, NotFound, ValidationProblem>
    > DeleteRecipeBookById(
        [FromRoute] string bookKey,
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
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > deleteResult = await recipeBookService.DeleteRecipeBookAsync(
            bookKey,
            userKey,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult.Result switch
        {
            EmptySuccessResult => TypedResults.Ok(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> => TypedResults.Forbid(),
            ValidationFailureResult error => TypedResults.ValidationProblem(error.Errors),
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
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
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        > createResult = await recipeBookService.CreateRecipeBookAsync(
            userKey,
            new() { Name = requestBody.Name, ShortDescription = requestBody.ShortDescription },
            cancellationToken
        );

        return createResult.Result switch
        {
            UserNotFoundResult => TypedResults.Forbid(),
            SuccessResult<RecipeBookDao> success => TypedResults.Created(
                (string?)null,
                new RecipeBookJsonResponse(success.Value)
            ),
            ValidationFailureResult validationError => TypedResults.ValidationProblem(
                validationError.Errors
            ),
            _ => throw new NotImplementedException(),
        };
    }

    #endregion CRUD Endpoints for Recipe Book

    #region Recipe Book Search Endpoints

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1068:CancellationToken parameters must come last",
        Justification = "This is a ASP.NET controller method"
    )]
    public async Task<
        Results<ValidationProblem, ForbidHttpResult, Ok<IEnumerable<RecipeBookJsonResponse>>>
    > GetBooks(
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken,
        [FromQuery(Name = "after-id")] string? afterId,
        [FromQuery(Name = "before-id")] string? beforeId,
        [FromQuery(Name = "result-ordering")] string? resultOrdering,
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
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.ListRecipeBooksAsync(
            userKey,
            new()
            {
                AfterBookKey = afterId,
                BeforeBookKey = beforeId,
                ResultOrder = recordOrdering.Value,
                ResultCount = Math.Min(100, Math.Max(1, pageSize)),
            },
            cancellationToken
        );

        return result.Result switch
        {
            SuccessResult<List<RecipeBookDao>> success => TypedResults.Ok(
                success.Value.Select(r => new RecipeBookJsonResponse(r))
            ),
            ValidationFailureResult validation => TypedResults.ValidationProblem(validation.Errors),
            UserNotFoundResult => TypedResults.Forbid(),
            _ => throw new NotImplementedException(),
        };
    }

    #endregion Recipe Book Search Endpoints

    #region Recipe Book Share Endpoints

    /// <summary>
    /// Regenerates the share key for a bok
    /// <br />
    /// POST /recipe-books/{bookKey}/shared-access/-/share-key?user={userKey}&kind={regenerate | clear}
    /// If-Match: {ifMatch}
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="kind">the type of regeneration kind</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="ifMatch">concurrency token</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpPost("{bookKey}/shared-access/-/share-key")]
    public async Task<
        Results<
            ValidationProblem,
            ForbidHttpResult,
            NotFound,
            Ok<RecipeBookJsonResponse>,
            PreconditionFailedHttpResult
        >
    > RegenerateShareKey(
        [FromQuery(Name = "user")] string userKey,
        [FromQuery(Name = "kind")] string kind,
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        BookShareKeyUpdateKind bookShareKeyUpdateKind;
        switch (kind)
        {
            case "regenerate":
                bookShareKeyUpdateKind = BookShareKeyUpdateKind.Regenerate;
                break;
            case "clear":
                bookShareKeyUpdateKind = BookShareKeyUpdateKind.Clear;
                break;
            default:
                return TypedResults.ValidationProblem([
                    new KeyValuePair<string, string[]>(
                        "kind",
                        [
                            $"kind must be either 'clear' or 'regenerate'. Value '{kind}' is not valid.",
                        ]
                    ),
                ]);
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > result = await recipeBookService.UpdateShareKeyAsync(
            bookKey,
            userKey,
            ifMatch.Value,
            bookShareKeyUpdateKind,
            cancellationToken
        );

        return result.Result switch
        {
            SuccessResult<RecipeBookDao> success => TypedResults.Ok(
                new RecipeBookJsonResponse(success.Value)
            ),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> => TypedResults.Forbid(),
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
            ValidationFailureResult validation => TypedResults.ValidationProblem(validation.Errors),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Gets the list of users with access
    /// <br />
    /// GET /recipe-books/{bookKey}/shared-access?user={userKey}
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpGet("{bookKey}/shared-access")]
    public async Task<
        Results<
            NotFound,
            ForbidHttpResult,
            ValidationProblem,
            Ok<IEnumerable<RecipeBookUserPermissionsJsonResponse>>
        >
    > GetSharedAccess(
        [FromQuery(Name = "user")] string userKey,
        [FromRoute] string bookKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookKey, userKey, cancellationToken);
        switch (result.Result)
        {
            case UserNotFoundResult:
                return TypedResults.Forbid();
            case Database.Results.NotFoundResult:
                return TypedResults.NotFound();
            case ValidationFailureResult error:
                return TypedResults.ValidationProblem(error.Errors);
            case SuccessResult<RecipeBookDao> success:
                if (!success.Value.MayManageAccess)
                {
                    return TypedResults.Forbid();
                }
                break;

            default:
                throw new NotImplementedException();
        }

        DatabaseResultVariant<
            Database.Results.NotFoundResult,
            SuccessResult<List<RecipeBookUserPermissionsDao>>,
            UserNotFoundResult,
            ForbiddenResult
        > listResult = await recipeBookService.ListUsersWithAccess(
            bookKey,
            userKey,
            cancellationToken
        );

        return listResult.Result switch
        {
            SuccessResult<List<RecipeBookUserPermissionsDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(k => new RecipeBookUserPermissionsJsonResponse(k))
            ),
            ForbiddenResult or UserNotFoundResult => TypedResults.Forbid(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Patches the list of users with access
    /// <br />
    /// PATCH /recipe-books/{bookKey}/shared-access?user={userKey}
    /// <br />
    /// <br />
    /// <c>
    /// {
    ///   "{userKey}" : RecipeBookUserPermissionsJsonRequest,
    ///   "{userKey}" : null, // delete entry
    ///   ...
    /// }
    /// </c>
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpPatch("{bookKey}/shared-access")]
    public async Task<
        Results<NotFound, ForbidHttpResult, NoContent, Conflict, ValidationProblem>
    > PatchSharedAccess(
        [FromQuery(Name = "user")] string userKey,
        [FromRoute] string bookKey,
        [FromBody] IEnumerable<KeyValuePair<string, RecipeBookUserPermissionsJsonRequest?>> body,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookKey, userKey, cancellationToken);

        switch (result.Result)
        {
            case Database.Results.UserNotFoundResult:
                return TypedResults.Forbid();
            case Database.Results.NotFoundResult:
                return TypedResults.NotFound();
            case ValidationFailureResult error:
                return TypedResults.ValidationProblem(error.Errors);
            case SuccessResult<RecipeBookDao> success:
                if (!success.Value.MayManageAccess)
                {
                    return TypedResults.Forbid();
                }
                DatabaseResultVariant<
                    EmptySuccessResult,
                    Database.Results.NotFoundResult,
                    DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
                    ValidationFailureResult,
                    Database.Results.ConflictResult
                > updateResult = await recipeBookService.UpdateUsersAccessAsync(
                    bookKey,
                    userKey,
                    body.Select(kv => new KeyValuePair<string, UpdateRecipeBookUserPermissionArgs?>(
                        kv.Key,
                        kv.Value is null
                            ? null
                            : new UpdateRecipeBookUserPermissionArgs()
                            {
                                MayViewBook = kv.Value.MayViewBook,
                                MayEditBook = kv.Value.MayEditBook,
                                Reviewed = kv.Value.Reviewed,
                            }
                    )),
                    cancellationToken
                );

                return updateResult.Result switch
                {
                    EmptySuccessResult => TypedResults.NoContent(),
                    Database.Results.NotFoundResult => TypedResults.NotFound(),
                    DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> =>
                        TypedResults.Forbid(),
                    Database.Results.ConflictResult => TypedResults.Conflict(),
                    ValidationFailureResult validation => TypedResults.ValidationProblem(
                        validation.Errors
                    ),
                    _ => throw new NotImplementedException(),
                };
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Allows a user to request access to a recipe book
    /// <br />
    /// POST /recipe-books/{bookKey}/shared-access/{userKey}?share-key={shareKey}
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="shareKey">the recipe book share key</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpPost("{bookKey}/shared-access/{userKey}")]
    public async Task<
        Results<
            NotFound,
            ForbidHttpResult,
            Ok<RequestAccessToRecipeBookStatusJsonResponse>,
            ValidationProblem,
            Conflict
        >
    > PostSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromQuery(Name = "share-key")] string shareKey,
        [FromRoute] string bookKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            Database.Results.ConflictResult,
            UserNotFoundResult
        > result = await recipeBookService.RequestAccessAsync(
            bookKey,
            userKey,
            shareKey,
            cancellationToken
        );

        return result.Result switch
        {
            Database.Results.UserNotFoundResult => TypedResults.Forbid(),
            Database.Results.ConflictResult => TypedResults.Conflict(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ValidationFailureResult error => TypedResults.ValidationProblem(error.Errors),
            SuccessResult<RecipeBookAccessRequestStatus>
            {
                Value: RecipeBookAccessRequestStatus value
            } => TypedResults.Ok(
                new RequestAccessToRecipeBookStatusJsonResponse()
                {
                    BookKey = bookKey,
                    Name = value.Name,
                    ShortDescription = value.ShortDescription,
                    Status = value.Status,
                }
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Deletes a user's request for recipe book access
    /// <br />
    /// DELETE /recipe-books/{bookKey}/shared-access/{userKey}
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpDelete("{bookKey}/shared-access/{userKey}")]
    public async Task<
        Results<NotFound, ForbidHttpResult, Ok, ValidationProblem, Conflict>
    > DeleteSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromRoute] string bookKey,
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
            ValidationFailureResult,
            UserNotFoundResult,
            Database.Results.ConflictResult
        > result = await recipeBookService.WithdrawAccessAsync(bookKey, userKey, cancellationToken);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.Forbid(),
            Database.Results.ConflictResult => TypedResults.Conflict(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            EmptySuccessResult => TypedResults.Ok(),
            ValidationFailureResult error => TypedResults.ValidationProblem(error.Errors),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Gets a user to requested access to a recipe book
    /// <br />
    /// GET /recipe-books/{bookKey}/shared-access/{userKey}?share-key={optional share key if first access}
    /// </summary>
    /// <param name="userKey">the user with share management permissions</param>
    /// <param name="bookKey">the book key</param>
    /// <param name="cancellationToken">cancels the request</param>
    /// <returns>http result</returns>
    [HttpGet("{bookKey}/shared-access/{userKey}")]
    public async Task<
        Results<
            NotFound,
            ValidationProblem,
            ForbidHttpResult,
            Ok<RequestAccessToRecipeBookStatusJsonResponse>
        >
    > GetSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromRoute] string bookKey,
        [FromQuery(Name = "share-key")] string? shareKey,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.GetAccessStatusAsync(
            bookKey,
            userKey,
            shareKey,
            cancellationToken
        );

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.Forbid(),
            ValidationFailureResult failure => TypedResults.ValidationProblem(failure.Errors),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            SuccessResult<RecipeBookAccessRequestStatus>
            {
                Value: RecipeBookAccessRequestStatus value
            } => TypedResults.Ok(
                new RequestAccessToRecipeBookStatusJsonResponse()
                {
                    BookKey = bookKey,
                    Name = value.Name,
                    ShortDescription = value.ShortDescription,
                    Status = value.Status,
                }
            ),
            _ => throw new NotImplementedException(),
        };
    }

    #endregion Recipe Book Share Endpoints
}
