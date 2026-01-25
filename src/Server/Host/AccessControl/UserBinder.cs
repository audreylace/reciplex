using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Binds data to <see cref="User"/>
/// </summary>
public class UserBinder : IModelBinder
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

        bindingContext.Result = ModelBindingResult.Success(new User() { UserId = subClaim.Value });
        return Task.CompletedTask;
    }
}
