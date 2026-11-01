using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Controller for users http api
/// </summary>
/// <param name="userServiceRepository">internal repository for user database objects</param>
/// <param name="stringIdProvider">service for marshaling ids to and from strings</param>
[ApiController]
[Route("users")]
[Authorize]
public class UsersController(
    IUsersService userServiceRepository,
    IStringIdProvider stringIdProvider
) : ControllerBase
{
    /// <summary>
    /// Returns list of accounts the current user has access to
    /// </summary>
    /// <param name="ct">request async cancellation token</param>
    /// <returns>List of use accounts</returns>
    [HttpGet]
    public async Task<Ok<IEnumerable<UserJsonResponse>>> HttpGetAccounts(CancellationToken ct)
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();
        return TypedResults.Ok(
            (await userServiceRepository.GetUsersBySubjectAsync(Authority, Subject, ct)).Select(
                ToUserJson
            )
        );
    }

    /// <summary>
    /// Gets information about a user
    /// </summary>
    /// <param name="userKey">the user string key</param>
    /// <param name="ct">request async cancellation token</param>
    /// <returns>controller http response</returns>
    /// <exception cref="NotImplementedException">thrown when the repository returns a response that is not handled</exception>
    [HttpGet("{userKey}")]
    public async Task<Results<Ok<UserJsonResponse>, NotFound>> HttpGetUser(
        [FromRoute] string userKey,
        CancellationToken ct
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult> result =
            await userServiceRepository.GetUserAsync(userId, ct);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            SuccessResult<UserDao> successResult => TypedResults.Ok(
                ToUserJson(successResult.Value)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a new user associated with the authenticated credentials
    /// </summary>
    /// <param name="userJsonBody">the new user body</param>
    /// <param name="ct">request async cancellation token</param>
    /// <returns>controller response</returns>
    /// <exception cref="NotImplementedException">thrown when the user service returns a result that this controller does not know how to handle</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<Results<Created<UserJsonResponse>, BadRequest>> HttpPostUser(
        [FromBody] UserJsonRequest userJsonBody,
        CancellationToken ct
    )
    {
        var (Authority, Subject) = HttpContext.RequireOpenIdConnectCredentials();
        DatabaseResultVariant<SuccessResult<UserDao>, ValidationFailureResult> createResult =
            await userServiceRepository.CreateUserAsync(
                new()
                {
                    DisplayName = userJsonBody.DisplayName,
                    Authority = Authority,
                    Subject = Subject,
                },
                ct
            );

        return createResult.Result switch
        {
            ValidationFailureResult => TypedResults.BadRequest(),
            SuccessResult<UserDao> success => TypedResults.Created(
                (string?)null,
                ToUserJson(success.Value)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Updates a user owned by this authenticated request
    /// </summary>
    /// <param name="userKey">the key of the user record to update</param>
    /// <param name="userJsonBody">the new values for the body</param>
    /// <param name="ifMatch">etag for opportunistic concurrency control</param>
    /// <param name="ct">request async cancellation token</param>
    /// <returns>controller response</returns>
    /// <exception cref="NotImplementedException">thrown when a response is returned by the user repository that this controller does not know how to handle</exception>
    [HttpPut("{userKey}")]
    [ValidateAntiForgeryToken]
    public async Task<
        Results<
            Ok<UserJsonResponse>,
            NotFound,
            BadRequest,
            PreconditionFailedHttpResult,
            ForbidHttpResult
        >
    > HttpPutUser(
        [FromRoute] string userKey,
        [FromBody] UserJsonRequest userJsonBody,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken ct
    )
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();

        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<UserDao>,
            UserNotFoundResult,
            ForbiddenResult
        > hasAccessCheck = await userServiceRepository.CheckAuthorizationAsync(
            Authority,
            Subject,
            userId,
            ct
        );

        switch (hasAccessCheck.Result)
        {
            case ForbiddenResult:
            case UserNotFoundResult:
                return TypedResults.NotFound();

            case SuccessResult<UserDao> successResult:

                if (successResult.Value.ConcurrencyTag != ifMatch.Value)
                {
                    return new PreconditionFailedHttpResult("If-Match");
                }

                DatabaseResultVariant<
                    SuccessResult<UserDao>,
                    ValidationFailureResult,
                    UserNotFoundResult,
                    Database.Results.ConflictResult
                > updateResult = await userServiceRepository.UpdateUserAsync(
                    userId,
                    ifMatch.Value,
                    new() { DisplayName = userJsonBody.DisplayName },
                    ct
                );

                return updateResult.Result switch
                {
                    SuccessResult<UserDao> success => TypedResults.Ok(ToUserJson(success.Value)),
                    ValidationFailureResult => TypedResults.BadRequest(),
                    UserNotFoundResult => TypedResults.NotFound(),
                    Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
                    _ => throw new NotImplementedException(),
                };

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Deletes the user record keyed by <paramref name="userKey"/>. The user record must be owned by the identify of the authenticated request.
    /// </summary>
    /// <param name="userKey">the user key of the record to delete</param>
    /// <param name="ifMatch">etag for opportunistic concurrency control</param>
    /// <param name="ct">request async cancellation token</param>
    /// <returns>controller response</returns>
    /// <exception cref="NotImplementedException">thrown when the user service returns a result that this controller does not know how to handle</exception>
    [HttpDelete("{userKey}")]
    [ValidateAntiForgeryToken]
    public async Task<
        Results<Ok, NotFound, ForbidHttpResult, PreconditionFailedHttpResult>
    > HttpDeleteUser(
        [FromRoute] string userKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken ct
    )
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();

        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.NotFound();
        }

        DatabaseResultVariant<
            SuccessResult<UserDao>,
            UserNotFoundResult,
            ForbiddenResult
        > hasAccessCheck = await userServiceRepository.CheckAuthorizationAsync(
            Authority,
            Subject,
            userId,
            ct
        );

        switch (hasAccessCheck.Result)
        {
            case ForbiddenResult:
                return TypedResults.NotFound();

            case UserNotFoundResult:
                return TypedResults.NotFound();

            case SuccessResult<UserDao> hasAccess:

                if (hasAccess.Value.ConcurrencyTag != ifMatch.Value)
                {
                    return new PreconditionFailedHttpResult("If-Match");
                }

                DatabaseResultVariant<
                    EmptySuccessResult,
                    Database.Results.ConflictResult,
                    UserNotFoundResult
                > result = await userServiceRepository.QueueUserDeleteAsync(
                    userId,
                    ifMatch.Value,
                    ct
                );

                return result.Result switch
                {
                    UserNotFoundResult => TypedResults.NotFound(),
                    EmptySuccessResult => TypedResults.Ok(),
                    Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
                    _ => throw new NotImplementedException(),
                };

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="IUserDao"/> into <see cref="UserJsonResponse"/>
    /// </summary>
    /// <param name="user">The <see cref="IUserDao"/> that will be converted into <see cref="UserJsonResponse"/></param>
    /// <returns>The resulting <see cref="UserJsonResponse"/></returns>
    public UserJsonResponse ToUserJson(UserDao user)
    {
        return new UserJsonResponse()
        {
            DisplayName = user.DisplayName,
            UserKey = stringIdProvider.AsString(user.Id),
            ConcurrencyTag = user.ConcurrencyTag,
        };
    }
}
