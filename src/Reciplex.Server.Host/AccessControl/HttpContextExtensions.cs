using System.IdentityModel.Tokens.Jwt;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host.AccessControl;

static class HttpContextExtensions
{
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

    public static (string Authority, string Subject) RequireOpenIdConnectCredentials(
        this HttpContext context
    )
    {
        var credentials =
            context.OpenIdConnectCredentials() ?? throw new InvalidOperationException();

        return credentials;
    }

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
