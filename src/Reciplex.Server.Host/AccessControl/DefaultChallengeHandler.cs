using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.AccessControl;

public class DefaultChallengeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Schema = "ChallengeIs401Schema";

    [Obsolete]
    public DefaultChallengeHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    )
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.Fail("Triggering 401"));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // 401 unauthorized
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
