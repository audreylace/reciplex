using System.IdentityModel.Tokens.Jwt;
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

    public static async Task<bool> IsUser(
        this HttpContext context,
        string userKey,
        CancellationToken ct
    )
    {
        (string authority, string subject) = context.RequireOpenIdConnectCredentials();
        AuthorizationCheckResult userCheck = await context
            .RequestServices.GetRequiredService<IUsersRepository>()
            .CheckAuthorizationAsync(authority, subject, userKey, ct);

        return userCheck switch
        {
            AuthorizationCheckResult.Authorized => true,
            AuthorizationCheckResult.NotFound or AuthorizationCheckResult.Forbidden => false,
            _ => throw new NotImplementedException(),
        };
    }
}
