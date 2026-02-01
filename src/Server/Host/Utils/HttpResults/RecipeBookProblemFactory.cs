using Microsoft.AspNetCore.Http.HttpResults;
using Reciplex.Server.Host.RecipeBookKeyUtils;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.Utils.HttpResults;

public class RecipeBookProblemFactory(IStringRecipeBookKeyInterop stringIdInterop)
    : IRecipeBookProblemFactory
{
    /// <summary>
    /// Returns a validation problem scoped to a book
    /// </summary>
    /// <param name="bookKey">The book key</param>
    /// <param name="errors">Map of validation errors</param>
    /// <returns>The validation problem</returns>
    public ValidationProblem BookValidationProblem(
        RecipeBookKey? bookKey,
        IDictionary<string, string[]>? errors
    )
    {
        string? bookId = bookKey is not null ? stringIdInterop.AsString(bookKey.Value) : null;
        string bookIdPart = bookKey is not null ? $" against recipe book \"{bookId}\" " : " ";

        return TypedResults.ValidationProblem(
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            title: "Validation Problem",
            detail: $"Requested operation{bookIdPart}failed because one or more validation errors",
            errors: errors ?? new Dictionary<string, string[]>(),
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }

    /// <summary>
    /// Generates a 412 pre-condition failure
    /// </summary>
    /// <param name="bookKey">the recipe book key</param>
    /// <param name="failedHeader">The header that triggered the failure</param>
    /// <returns>the 412 result</returns>
    public ProblemHttpResult BookPreconditionFailed(RecipeBookKey bookKey, string failedHeader)
    {
        string bookId = stringIdInterop.AsString(bookKey);
        return TypedResults.Problem(
            statusCode: 412,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-412-precondition-failed",
            title: "Pre-Condition Failure",
            detail: $"Requested operation against recipe book \"{bookId}\" failed because one or more conditions in the request headers could not be satisfied.",
            extensions:
            [
                new KeyValuePair<string, object?>("bookId", bookId),
                new KeyValuePair<string, object?>("failedHeader", failedHeader),
            ]
        );
    }

    /// <summary>
    /// Generates a 403 forbidden error
    /// </summary>
    /// <param name="bookKey">the recipe book key</param>
    /// <returns>the 404 result</returns>
    public ProblemHttpResult OperationOnBookForbidden(RecipeBookKey bookKey)
    {
        string bookId = stringIdInterop.AsString(bookKey);
        return TypedResults.Problem(
            statusCode: 403,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
            title: "Operation on Recipe Book Forbidden",
            detail: $"Caller lacks required permissions to perform requested action on recipe book \"{bookId}\".",
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }

    /// <summary>
    /// Generates a 404 error
    /// </summary>
    /// <param name="bookKey">the recipe book key</param>
    /// <returns>the 404 result</returns>
    public ProblemHttpResult BookNotFoundResult(RecipeBookKey bookKey)
    {
        string bookId = stringIdInterop.AsString(bookKey);
        return TypedResults.Problem(
            statusCode: 404,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            title: "Recipe Book Not Found",
            detail: $"Recipe book \"{bookId}\" was either not found or caller does not have access.",
            extensions: [new KeyValuePair<string, object?>("bookId", bookId)]
        );
    }
}
