namespace Reciplex.Server.Host;

public class PreconditionFailedHttpResult(string failedHeader) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return TypedResults
            .Problem(
                statusCode: 412,
                type: "https://datatracker.ietf.org/doc/html/rfc9110#name-412-precondition-failed",
                title: "Pre-Condition Failure",
                detail: $"Requested operation failed because one or more conditions in the request headers could not be satisfied.",
                extensions: [new KeyValuePair<string, object?>("failedHeader", failedHeader)]
            )
            .ExecuteAsync(httpContext);
    }
}
