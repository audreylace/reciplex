using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Endpoints for working with a recipe book
/// </summary>
/// <param name="recipeBookService">Provides services for manipulating recipe books</param>
/// <param name="stringIdProvider">service for marshalling ids to and from strings</param>
[ApiController]
[Route("recipe-books")]
[Authorize]
public class RecipeBooksController(
    IRecipeBooksService recipeBookService,
    IStringIdProvider stringIdProvider
) : ControllerBase
{
    #region CRUD Endpoints for Recipe Book

    /// <summary>
    /// Gets a book by ID
    /// <pre><code>
    /// GET /recipe-books/{bookKey}?user={userKey}
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The key of the book</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token cancelled when the remote closes their connection</param>
    /// <returns>A HTTP response indicating the outcome of the operation</returns>
    [HttpGet("{bookKey}")]
    public async Task<Results<Ok<RecipeBookJsonResponse>, NotFound>> GetBookById(
        [FromRoute] string bookKey,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        // do these first to avoid extra database round trip
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookId, userId, cancellationToken);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            SuccessResult<RecipeBookDao> { Value: RecipeBookDao book } => TypedResults.Ok(
                ToJson(book)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Updates a recipe book <br />
    /// <pre><code>
    /// PUT /recipe-books/{bookKey}?user={userKey}
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<BadRequest, Ok<RecipeBookJsonResponse>, NotFound, PreconditionFailedHttpResult>
    > UpdateBookById(
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        [FromBody] RecipeBookJsonRequest requestBody,
        CancellationToken cancellationToken
    )
    {
        // do these first to avoid extra database round trip
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > updateResult = await recipeBookService.UpdateRecipeBookAsync(
            bookId,
            userId,
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
            SuccessResult<RecipeBookDao> success => TypedResults.Ok(ToJson(success.Value)),

            Database.Results.NotFoundResult
            or DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> =>
                TypedResults.NotFound(),

            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),

            ValidationFailureResult => TypedResults.BadRequest(),

            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Deletes a recipe book <br />
    /// <pre><code>
    /// DELETE /recipe-books/{bookKey}?user={userKey}
    /// If-Match: {ifMatch}
    /// </code></pre>
    /// </summary>
    /// <param name="bookKey">The id of the recipe book</param>
    /// <param name="ifMatch">The recipe book etag</param>
    /// <param name="userKey">Key of the user performing the action</param>
    /// <param name="cancellationToken">token that cancels when the connection is closed</param>
    /// <returns>Task that resolves to the http response</returns>
    [HttpDelete("{bookKey}")]
    [ValidateAntiForgeryToken]
    public async Task<Results<Ok, PreconditionFailedHttpResult, NotFound>> DeleteRecipeBookById(
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        [FromQuery(Name = "user")] string userKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            EmptySuccessResult,
            Database.Results.NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            Database.Results.ConflictResult
        > deleteResult = await recipeBookService.QueueRecipeBookDeleteAsync(
            bookId,
            userId,
            ifMatch.Value,
            cancellationToken
        );

        return deleteResult.Result switch
        {
            EmptySuccessResult => TypedResults.Ok(),
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>
            or Database.Results.NotFoundResult => TypedResults.NotFound(),
            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a recipe book <br />
    /// <pre><code>
    /// POST /recipe-books?user={userKey}
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<ForbidHttpResult, Created<RecipeBookJsonResponse>, BadRequest>
    > CreateBook(
        [FromQuery(Name = "user")] [BindRequired] string userKey,
        [FromBody] RecipeBookJsonRequest requestBody,
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

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        > createResult = await recipeBookService.CreateRecipeBookAsync(
            userId,
            new() { Name = requestBody.Name, ShortDescription = requestBody.ShortDescription },
            cancellationToken
        );

        return createResult.Result switch
        {
            UserNotFoundResult => TypedResults.Forbid(),
            SuccessResult<RecipeBookDao> success => TypedResults.Created(
                (string?)null,
                ToJson(success.Value)
            ),
            ValidationFailureResult => TypedResults.BadRequest(),
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
    /// GET /recipe-books?result-ordering={id-increasing | id-decreasing}&amp;page-size={pageSize}&amp;user={userKey}
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
        Results<BadRequest, ForbidHttpResult, Ok<IEnumerable<RecipeBookJsonResponse>>>
    > GetBooks(
        [FromQuery(Name = "user")] string userKey,
        [FromQuery(Name = "after-id")] string? afterIdString,
        [FromQuery(Name = "before-id")] string? beforeIdString,
        [FromQuery(Name = "result-ordering")] string? resultOrdering,
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

        const string OrderIdIncreasing = "id-increasing";
        const string OrderIdDecreasing = "id-decreasing";

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
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.ListRecipeBooksAsync(
            userId,
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
                success.Value.Select(ToJson)
            ),
            ValidationFailureResult validation => TypedResults.BadRequest(),
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<BadRequest, NotFound, Ok<RecipeBookJsonResponse>, PreconditionFailedHttpResult>
    > RegenerateShareKey(
        [FromQuery(Name = "user")] string userKey,
        [FromQuery(Name = "kind")] string kind,
        [FromRoute] string bookKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
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
                return TypedResults.BadRequest();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            Database.Results.ConflictResult
        > result = await recipeBookService.UpdateShareKeyAsync(
            bookId,
            userId,
            ifMatch.Value,
            bookShareKeyUpdateKind,
            cancellationToken
        );

        return result.Result switch
        {
            SuccessResult<RecipeBookDao> success => TypedResults.Ok(ToJson(success.Value)),

            Database.Results.NotFoundResult
            or DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> =>
                TypedResults.NotFound(),

            Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),

            ValidationFailureResult => TypedResults.BadRequest(),

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
        Results<NotFound, ForbidHttpResult, Ok<IEnumerable<RecipeBookUserPermissionsJsonResponse>>>
    > GetSharedAccess(
        [FromQuery(Name = "user")] string userKey,
        [FromRoute] string bookKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }
        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }
        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookId, userId, cancellationToken);
        switch (result.Result)
        {
            case Database.Results.NotFoundResult:
            case UserNotFoundResult:
                return TypedResults.NotFound();

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
            bookId,
            userId,
            cancellationToken
        );

        return listResult.Result switch
        {
            SuccessResult<List<RecipeBookUserPermissionsDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(ToPermissionJson)
            ),

            ForbiddenResult or UserNotFoundResult or Database.Results.NotFoundResult =>
                TypedResults.NotFound(),

            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Constructs JSON response from <see cref="RecipeBookUserPermissionsDao"/>
    /// </summary>
    /// <param name="dao">the dao to convert</param>
    public RecipeBookUserPermissionsJsonResponse ToPermissionJson(RecipeBookUserPermissionsDao dao)
    {
        return new RecipeBookUserPermissionsJsonResponse()
        {
            UserKey = stringIdProvider.AsString(dao.UserId),
            BookKey = stringIdProvider.AsString(dao.BookId),
            MayViewBook = dao.MayViewBook,
            MayEditBook = dao.MayEditBook,
            Reviewed = dao.Reviewed,
            UserDisplayName = dao.UserDisplayName,
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<NotFound, ForbidHttpResult, NoContent, Conflict, BadRequest>
    > PatchSharedAccess(
        [FromQuery(Name = "user")] string userKey,
        [FromRoute] string bookKey,
        [FromBody] IEnumerable<KeyValuePair<string, RecipeBookUserPermissionsJsonRequest?>> body,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }
        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }
        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        List<KeyValuePair<long, UpdateRecipeBookUserPermissionArgs?>> entriesToPatch = [];
        foreach (var kv in body)
        {
            if (stringIdProvider.AsLong(kv.Key) is not long userIdKey || userIdKey < 0)
            {
                return TypedResults.NotFound();
            }

            entriesToPatch.Add(
                new(
                    userIdKey,
                    kv.Value is not null
                        ? new UpdateRecipeBookUserPermissionArgs()
                        {
                            MayViewBook = kv.Value.MayViewBook,
                            MayEditBook = kv.Value.MayEditBook,
                            Reviewed = kv.Value.Reviewed,
                        }
                        : null
                )
            );
        }

        if (entriesToPatch.Count < 1)
        {
            return TypedResults.NoContent();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            Database.Results.NotFoundResult,
            UserNotFoundResult
        > result = await recipeBookService.GetRecipeBookAsync(bookId, userId, cancellationToken);

        switch (result.Result)
        {
            case UserNotFoundResult:
            case Database.Results.NotFoundResult:
                return TypedResults.NotFound();
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
                    bookId,
                    userId,
                    entriesToPatch,
                    cancellationToken
                );

                return updateResult.Result switch
                {
                    EmptySuccessResult => TypedResults.NoContent(),
                    Database.Results.NotFoundResult
                    or DatabaseResultVariant<ForbiddenResult, UserNotFoundResult> =>
                        TypedResults.NotFound(),

                    Database.Results.ConflictResult => TypedResults.Conflict(),
                    ValidationFailureResult => TypedResults.BadRequest(),
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
    [ValidateAntiForgeryToken]
    public async Task<
        Results<NotFound, Ok<RequestAccessToRecipeBookStatusJsonResponse>, BadRequest, Conflict>
    > PostSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromQuery(Name = "share-key")] string shareKey,
        [FromRoute] string bookKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            Database.Results.ConflictResult,
            UserNotFoundResult
        > result = await recipeBookService.RequestAccessAsync(
            bookId,
            userId,
            shareKey,
            cancellationToken
        );

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            Database.Results.ConflictResult => TypedResults.Conflict(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            ValidationFailureResult => TypedResults.BadRequest(),
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
    [ValidateAntiForgeryToken]
    public async Task<Results<NotFound, Ok, BadRequest, Conflict>> DeleteSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromRoute] string bookKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            EmptySuccessResult,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult,
            Database.Results.ConflictResult
        > result = await recipeBookService.WithdrawAccessAsync(bookId, userId, cancellationToken);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            Database.Results.ConflictResult => TypedResults.Conflict(),
            Database.Results.NotFoundResult => TypedResults.NotFound(),
            EmptySuccessResult => TypedResults.Ok(),
            ValidationFailureResult error => TypedResults.BadRequest(),
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
        Results<NotFound, BadRequest, Ok<RequestAccessToRecipeBookStatusJsonResponse>>
    > GetSelfServiceSharedAccess(
        [FromRoute] string userKey,
        [FromRoute] string bookKey,
        [FromQuery(Name = "share-key")] string? shareKey,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }
        if (stringIdProvider.AsLong(bookKey) is not long bookId || bookId < 0)
        {
            return TypedResults.NotFound();
        }
        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            Database.Results.NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        > result = await recipeBookService.GetAccessStatusAsync(
            bookId,
            userId,
            shareKey,
            cancellationToken
        );

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            ValidationFailureResult failure => TypedResults.BadRequest(),
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

    /// <summary>
    /// Builds an instance of <see cref="RecipeBookJsonResponse"/>
    /// </summary>
    /// <param name="book">the dao data to convert</param>
    private RecipeBookJsonResponse ToJson(RecipeBookDao book)
    {
        return new RecipeBookJsonResponse()
        {
            BookKey = stringIdProvider.AsString(book.Id),
            Name = book.Name,
            ShortDescription = book.ShortDescription,
            OwningUserKey = stringIdProvider.AsString(book.OwningUserId),
            Created = book.Created,
            LastModified = book.LastModified,
            ConcurrencyTag = book.ConcurrencyTag,
            MayEdit = book.MayEditBook,
            MayDelete = book.MayDeleteBook,
            MayShare = book.MayShareBook,
            MayManageAccess = book.MayManageAccess,
            ShareKey = book.MayShareBook ? book.ShareKey : null,
        };
    }
}
