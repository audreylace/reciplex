using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// Controller for getting XSRF tokens
/// </summary>
/// <param name="options">application routing options</param>
/// <param name="antiforgery">anti forgery service invoked to reset the token at the start of the OIDC flow</param>
[ApiController]
[Route("xsrf")]
public class XsrfController(IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>
    /// Post endpoint to get the xsrf tokens since it causes them to regenerate
    /// </summary>
    /// <param name="header">header set to magic value</param>
    /// <returns>the xsrf token or a problem result</returns>
    [HttpPost]
    public Results<Ok<XsrfResponse>, BadRequest> PostGetXsrfToken(
        [FromHeader(Name = "X-Requested-With")] string? header
    )
    {
        if (header != "reciplex js")
        {
            return TypedResults.BadRequest();
        }
        var token = antiforgery.GetAndStoreTokens(HttpContext);
        return TypedResults.Ok<XsrfResponse>(new() { XsrfToken = token.RequestToken ?? "" });
    }
}
