using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Controller for performing a user signout
/// </summary>
/// <param name="antiforgery">anti forgery service invoked to reset the token at the start of the OIDC flow</param>
[ApiController]
[Route("sign-out")]
[ValidateAntiForgeryToken]
[Authorize]
public class SignOutController(IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>
    /// Kill's the client side session cookie
    /// </summary>
    /// <returns>sign out result/returns>
    [HttpPost]
    public SignOutHttpResult PostSignOut()
    {
        antiforgery.GetAndStoreTokens(HttpContext); // change tokens
        return TypedResults.SignOut(new() { }, [CookieAuthenticationDefaults.AuthenticationScheme]);
    }
}
