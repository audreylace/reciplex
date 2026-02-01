using Microsoft.AspNetCore.Http.HttpResults;

namespace Reciplex.Server.Host.Utils.HttpResults;

/// <summary>
/// Custom <see cref="ProblemHttpResult"/> for this application
/// </summary>
public static class CustomProblemHttpResults
{
    /// <summary>
    /// Generates an internal server error
    /// </summary>
    /// <param name="exceptionMessage">Exception message</param>
    /// <param name="stackTrace">Stack trace</param>
    /// <returns>the 500 error</returns>
    public static ProblemHttpResult InternalServerError(
        string? exceptionMessage = null,
        string? stackTrace = null
    )
    {
        List<KeyValuePair<string, object?>> extensions = [];

        if (exceptionMessage is not null)
        {
            extensions.Add(new("exceptionMessage", exceptionMessage));
        }

        if (stackTrace is not null)
        {
            extensions.Add(new("stackTrace", stackTrace));
        }

        return TypedResults.Problem(
            statusCode: 500,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error",
            title: "Internal Server Error",
            detail: $"The server was unable to fullfil the request because something went wrong.",
            extensions: extensions
        );
    }

    /// <summary>
    /// Generates a 403 forbidden error
    /// </summary>
    /// <returns>the 403 result</returns>
    public static ProblemHttpResult OperationForbidden()
    {
        return TypedResults.Problem(
            statusCode: 403,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
            title: "Operation Forbidden",
            detail: "Caller lacks required permissions to perform requested action."
        );
    }
}
