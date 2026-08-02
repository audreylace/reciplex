using System.IdentityModel.Tokens.Jwt;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Static extension methods on <see cref="HttpContext"/> for working with
/// authentication and authorization.
/// </summary>
static class HttpContextExtensions
{
    /// <summary>
    /// Gets the user's OIDC credentials or null if there are none
    /// </summary>
    /// <param name="context">the current request context</param>
    /// <returns>the user's authority and subject or null if the request does not have them</returns>
    public static (string Authority, string Subject)? OpenIdConnectCredentials(
        this HttpContext context
    )
    {
        var user = context.User;
        if (user is null)
        {
            return null;
        }

        (string? authority, string? subject) = user
            .Identities.Select(
                (identity) =>
                {
                    string? issuer = identity.FindFirst(JwtRegisteredClaimNames.Iss)?.Value;
                    string? subject = identity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                    return (issuer, subject);
                }
            )
            .FirstOrDefault(
                (arg) =>
                    !string.IsNullOrWhiteSpace(arg.issuer)
                    && !string.IsNullOrWhiteSpace(arg.subject)
            );

        if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(subject))
        {
            return null;
        }

        return (authority, subject);
    }

    /// <summary>
    /// Gets credentials via <see cref="OpenIdConnectCredentials"/> throwing if they are not present
    /// </summary>
    /// <param name="context">the current request context</param>
    /// <returns>the user's credentials</returns>
    /// <exception cref="InvalidOperationException">thrown if the credentials are missing from the request</exception>
    public static (string Authority, string Subject) RequireOpenIdConnectCredentials(
        this HttpContext context
    )
    {
        var credentials =
            context.OpenIdConnectCredentials() ?? throw new InvalidOperationException();

        return credentials;
    }

    /// <summary>
    /// Checks if the request credentials grants access to <paramref name="userKey"/>
    /// </summary>
    /// <param name="context">the current request context</param>
    /// <param name="userKey">the requested key</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>result of the check</returns>
    public static async Task<bool> RequestHasAccessToUserKey(
        this HttpContext context,
        string userKey,
        CancellationToken ct
    )
    {
        (string authority, string subject) = context.RequireOpenIdConnectCredentials();
        DatabaseResultVariant<
            SuccessResult<UserDao>,
            UserNotFoundResult,
            ForbiddenResult
        > userCheck = await context
            .RequestServices.GetRequiredService<IUsersService>()
            .CheckAuthorizationAsync(authority, subject, userKey, ct);

        return userCheck.Result switch
        {
            SuccessResult<UserDao> => true,
            UserNotFoundResult or ForbiddenResult => false,
            _ => throw new NotImplementedException(),
        };
    }
}
