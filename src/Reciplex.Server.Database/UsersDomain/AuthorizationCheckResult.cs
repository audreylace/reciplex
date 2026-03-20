namespace Reciplex.Server.Database.UsersDomain;

public abstract record AuthorizationCheckResult
{
    public record Authorized(string ConcurrencyTag) : AuthorizationCheckResult;

    public record NotFound() : AuthorizationCheckResult;

    public record Forbidden(string ConcurrencyTag) : AuthorizationCheckResult;
}
