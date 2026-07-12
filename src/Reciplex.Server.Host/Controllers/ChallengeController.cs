using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Begins an authentication flow by issuing a challenge
/// </summary>
/// <param name="options">app wide options controlling the authentication flow</param>
[ApiController]
[Route("challenge")]
public class ChallengeController(IOptions<RoutingOptions> options) : ControllerBase
{
    /// <summary>
    /// The challenge endpoint that begins an authentication flow
    /// </summary>
    /// <returns>Challenge result</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <see cref="AuthenticationFlowOptions.PostSignInPath"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <see cref="AuthenticationFlowOptions.PostSignInPath"/> is empty or whitespace.
    /// </exception>
    [HttpGet]
    public ChallengeHttpResult Navigate()
    {
        UriBuilder uriBuilder = new(options.Value.Domain) { Path = "/accounts/-/select" };
        return TypedResults.Challenge(
            new() { RedirectUri = uriBuilder.Uri.OriginalString },
            [OpenIdConnectDefaults.AuthenticationScheme]
        );
    }

    /// <summary>
    /// Gets the current user's authentication status
    /// </summary>
    /// <returns>the http result</returns>
    [HttpGet("inspect")]
    public Ok<ChallengeJsonResponse> GetStatus()
    {
        (string Authority, string Subject)? credentials = HttpContext.OpenIdConnectCredentials();
        if (credentials is null)
        {
            return TypedResults.Ok(new ChallengeJsonResponse() { ChallengeRequired = true });
        }

        return TypedResults.Ok(new ChallengeJsonResponse() { ChallengeRequired = false });
    }
}
