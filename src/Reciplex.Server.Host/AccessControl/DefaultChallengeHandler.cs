using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Challenge handler for any routes requiring authentication. Returns 401 error.
/// </summary>
public class DefaultChallengeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Default authentication schema key
    /// </summary>
    public const string Schema = "ChallengeIs401Schema";

#pragma warning disable CA1041 // Provide ObsoleteAttribute message
    [Obsolete]
#pragma warning restore CA1041 // Provide ObsoleteAttribute message
    public DefaultChallengeHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    )
        : base(options, logger, encoder, clock) { }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.Fail("Triggering 401"));
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // 401 unauthorized
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
