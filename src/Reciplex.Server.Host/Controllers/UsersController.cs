using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.UsersDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(IUsersRepository userServiceRepository) : ControllerBase
{
    /// <summary>
    /// Returns list of accounts the current user has access to
    /// </summary>
    /// <returns>List of use accounts</returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public Ok<IAsyncEnumerable<UserJsonResponse>> GetAccounts()
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();
        return TypedResults.Ok(
            userServiceRepository
                .GetUsersBySubjectAsync(Authority, Subject)
                .Select(u => new UserJsonResponse(u))
        );
    }

    [HttpGet("{userKey}")]
    public async Task<Results<Ok<UserJsonResponse>, NotFound>> GetUser(
        [FromRoute] string userKey,
        CancellationToken ct
    )
    {
        UserDao? user = await userServiceRepository.GetUserAsync(userKey, ct);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new UserJsonResponse(user));
    }

    [HttpPost]
    public async Task<
        Results<Created<UserJsonResponse>, NotFound, InternalServerError, ValidationProblem>
    > PostUser([FromBody] UserJsonRequest userJsonBody, CancellationToken ct)
    {
        var (Authority, Subject) = HttpContext.RequireOpenIdConnectCredentials();
        CreateUserResult createResult = await userServiceRepository.CreateUserAsync(
            new()
            {
                DisplayName = userJsonBody.DisplayName,
                Authority = Authority,
                Subject = Subject,
            },
            ct
        );

        return createResult switch
        {
            CreateUserResult.ValidationFailure validationFailure => TypedResults.ValidationProblem(
                validationFailure.Errors
            ),
            CreateUserResult.Success success => TypedResults.Created(
                (string?)null,
                new UserJsonResponse(success.User)
            ),
            _ => TypedResults.InternalServerError(),
        };
    }

    [HttpPut("{userKey}")]
    public async Task<
        Results<
            Ok<UserJsonResponse>,
            NotFound,
            InternalServerError,
            ValidationProblem,
            PreconditionFailedHttpResult,
            ForbidHttpResult
        >
    > PutUser(
        [FromRoute] string userKey,
        [FromBody] UserJsonRequest userJsonBody,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken ct
    )
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();

        AuthorizationCheckResult hasAccessCheck =
            await userServiceRepository.CheckAuthorizationAsync(Authority, Subject, userKey, ct);

        switch (hasAccessCheck)
        {
            case AuthorizationCheckResult.Forbidden:
                return TypedResults.Forbid();

            case AuthorizationCheckResult.NotFound:
                return TypedResults.NotFound();

            case AuthorizationCheckResult.Authorized hasAccess:

                if (hasAccess.ConcurrencyTag != ifMatch.Value)
                {
                    return new PreconditionFailedHttpResult("If-Match");
                }

                UpdateUserResult result = await userServiceRepository.UpdateUserAsync(
                    userKey,
                    ifMatch.Value,
                    new() { DisplayName = userJsonBody.DisplayName },
                    ct
                );

                return result switch
                {
                    UpdateUserResult.Success success => TypedResults.Ok(
                        new UserJsonResponse(success.User)
                    ),
                    UpdateUserResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
                    _ => TypedResults.InternalServerError(),
                };

            default:
                return TypedResults.InternalServerError();
        }
    }

    [HttpDelete("{userKey}")]
    public async Task<
        Results<Ok, NotFound, ForbidHttpResult, PreconditionFailedHttpResult, InternalServerError>
    > DeleteUser(
        [FromRoute] string userKey,
        [BindRequired] [FromHeader(Name = "If-Match")] EtagValue ifMatch,
        CancellationToken ct
    )
    {
        (string Authority, string Subject) = HttpContext.RequireOpenIdConnectCredentials();

        AuthorizationCheckResult hasAccessCheck =
            await userServiceRepository.CheckAuthorizationAsync(Authority, Subject, userKey, ct);

        switch (hasAccessCheck)
        {
            case AuthorizationCheckResult.Forbidden:
                return TypedResults.Forbid();

            case AuthorizationCheckResult.NotFound:
                return TypedResults.NotFound();

            case AuthorizationCheckResult.Authorized hasAccess:

                if (hasAccess.ConcurrencyTag != ifMatch.Value)
                {
                    return new PreconditionFailedHttpResult("If-Match");
                }

                DeleteUserResult result = await userServiceRepository.DeleteUserAsync(
                    userKey,
                    ifMatch.Value,
                    ct
                );

                return result switch
                {
                    DeleteUserResult.Success => TypedResults.Ok(),
                    DeleteUserResult.Conflict => new PreconditionFailedHttpResult("If-Match"),
                    _ => TypedResults.InternalServerError(),
                };

            default:
                return TypedResults.InternalServerError();
        }
    }
}
