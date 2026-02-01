using Microsoft.AspNetCore.Http.HttpResults;
using Reciplex.Server.Host.RecipeKeyUtils;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Utils.HttpResults;

/// <summary>
/// Implements <see cref="IRecipeProblemFactory"/>
/// </summary>
/// <param name="IStringRecipeKeyInterop">API for creating strings ids from the recipe's surrogate key</param>
public class RecipeProblemFactory(IStringRecipeKeyInterop stringIdInterop) : IRecipeProblemFactory
{
    /// <inheritdoc />
    public ProblemHttpResult OperationOnRecipeForbidden(RecipeKey recipeKey)
    {
        string recipeId = stringIdInterop.AsString(recipeKey);
        return TypedResults.Problem(
            statusCode: 403,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
            title: "Operation on Recipe Forbidden",
            detail: $"Caller lacks required permissions to perform requested action on recipe \"{recipeId}\".",
            extensions: [new KeyValuePair<string, object?>("recipeId", recipeId)]
        );
    }

    /// <inheritdoc />
    public ProblemHttpResult RecipeNotFoundResult(RecipeKey recipeKey)
    {
        string recipeId = stringIdInterop.AsString(recipeKey);
        return TypedResults.Problem(
            statusCode: 404,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            title: "Recipe Not Found",
            detail: $"Recipe \"{recipeId}\" was either not found or caller does not have access.",
            extensions: [new KeyValuePair<string, object?>("recipeId", recipeId)]
        );
    }

    /// <inheritdoc />
    public ProblemHttpResult RecipePreconditionFailed(RecipeKey recipeKey, string failedHeader)
    {
        string recipeId = stringIdInterop.AsString(recipeKey);
        return TypedResults.Problem(
            statusCode: 412,
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-412-precondition-failed",
            title: "Pre-Condition Failure",
            detail: $"Requested operation against recipe \"{recipeId}\" failed because one or more conditions in the request headers could not be satisfied.",
            extensions:
            [
                new KeyValuePair<string, object?>("recipeId", recipeId),
                new KeyValuePair<string, object?>("failedHeader", failedHeader),
            ]
        );
    }

    /// <inheritdoc />
    public ValidationProblem RecipeValidationProblem(
        RecipeKey? recipeKey,
        IDictionary<string, string[]>? errors
    )
    {
        string? recipeKeyString = null;
        List<KeyValuePair<string, object?>> extensions = [];
        if (recipeKey is not null)
        {
            recipeKeyString = stringIdInterop.AsString(recipeKey.Value);
            extensions.Add(new KeyValuePair<string, object?>("recipeId", recipeKeyString));
        }
        string recipeIdPart = recipeKeyString is not null
            ? $"against recipe \"{recipeKeyString}\" "
            : "";
        return TypedResults.ValidationProblem(
            type: "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            title: "Validation Problem",
            detail: $"Requested operation {recipeIdPart}failed because of one or more validation errors",
            errors: errors ?? new Dictionary<string, string[]>(),
            extensions: extensions
        );
    }
}
