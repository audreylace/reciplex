using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(IUsersService userServiceRepository) : ControllerBase
{
    /// <summary>
    /// Returns list of accounts the current user has access to
    /// </summary>
    /// <returns>List of use accounts</returns>
    [HttpGet]
    public async Task<Ok<IEnumerable<UserJsonResponse>>> HttpGetAccounts(CancellationToken ct)
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();
        return TypedResults.Ok(
            (await userServiceRepository.GetUsersBySubjectAsync(Authority, Subject, ct)).Select(
                u => new UserJsonResponse(u)
            )
        );
    }

    [HttpGet("{userKey}")]
    public async Task<Results<Ok<UserJsonResponse>, NotFound>> HttpGetUser(
        [FromRoute] string userKey,
        CancellationToken ct
    )
    {
        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult> result =
            await userServiceRepository.GetUserAsync(userKey, ct);

        return result.Result switch
        {
            UserNotFoundResult => TypedResults.NotFound(),
            SuccessResult<UserDao> successResult => TypedResults.Ok(
                new UserJsonResponse(successResult.Value)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<
        Results<Created<UserJsonResponse>, NotFound, InternalServerError, ValidationProblem>
    > HttpPostUser([FromBody] UserJsonRequest userJsonBody, CancellationToken ct)
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
            ValidationFailureResult validationFailure => TypedResults.ValidationProblem(
                validationFailure.Errors
            ),
            SuccessResult<UserDao> success => TypedResults.Created(
                (string?)null,
                new UserJsonResponse(success.Value)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    [HttpPut("{userKey}")]
    [ValidateAntiForgeryToken]
    public async Task<
        Results<
            Ok<UserJsonResponse>,
            NotFound,
            InternalServerError,
            ValidationProblem,
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

        DatabaseResultVariant<
            SuccessResult<UserDao>,
            UserNotFoundResult,
            ForbiddenResult
        > hasAccessCheck = await userServiceRepository.CheckAuthorizationAsync(
            Authority,
            Subject,
            userKey,
            ct
        );

        switch (hasAccessCheck.Result)
        {
            case ForbiddenResult:
                return TypedResults.Forbid();

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
                    userKey,
                    ifMatch.Value,
                    new() { DisplayName = userJsonBody.DisplayName },
                    ct
                );

                return updateResult.Result switch
                {
                    SuccessResult<UserDao> success => TypedResults.Ok(
                        new UserJsonResponse(success.Value)
                    ),
                    ValidationFailureResult validationFailureResult =>
                        TypedResults.ValidationProblem(validationFailureResult.Errors),
                    UserNotFoundResult => TypedResults.NotFound(),
                    Database.Results.ConflictResult => new PreconditionFailedHttpResult("If-Match"),
                    _ => throw new NotImplementedException(),
                };

            default:
                throw new NotImplementedException();
        }
    }

    [HttpDelete("{userKey}")]
    [ValidateAntiForgeryToken]
    public async Task<
        Results<Ok, NotFound, ForbidHttpResult, PreconditionFailedHttpResult, InternalServerError>
    > HttpDeleteUser(
        [FromRoute] string userKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken ct
    )
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();

        DatabaseResultVariant<
            SuccessResult<UserDao>,
            UserNotFoundResult,
            ForbiddenResult
        > hasAccessCheck = await userServiceRepository.CheckAuthorizationAsync(
            Authority,
            Subject,
            userKey,
            ct
        );

        switch (hasAccessCheck.Result)
        {
            case ForbiddenResult:
                return TypedResults.Forbid();

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
                > result = await userServiceRepository.DeleteUserAsync(userKey, ifMatch.Value, ct);

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
}
