using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Reciplex.Server.Host.Utils.UserKeyUtils;

/// <summary>
/// Binds data to <see cref="ApplicationClaimsPrincipal"/>
/// </summary>
public class ApplicationClaimsPrincipalBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        HttpContext httpContext = bindingContext.HttpContext;

        if (httpContext.User is null)
        {
            return Task.CompletedTask;
        }

        // todo - configure this to pull from X identities looking for Y claims
        Claim? subClaim = httpContext.User.FindFirst("sub");
        if (subClaim is null)
        {
            return Task.CompletedTask;
        }

        // todo - map the subject to the user id
        if (!long.TryParse(subClaim.Value, out long userId))
        {
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(
            new ApplicationClaimsPrincipal() { UserKey = new(userId) }
        );
        return Task.CompletedTask;
    }
}
