using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// API for searching recipes
/// </summary>
/// <param name="recipeService">service for getting recipes</param>
[ApiController]
[Route("recipes-search")]
[Authorize]
public class RecipesSearchController(IRecipesService recipeService) : ControllerBase
{
    [HttpGet]
    public async Task<
        Results<
            Ok<IEnumerable<RecipeListEntryJsonResponse>>,
            NotFound,
            ForbidHttpResult,
            ValidationProblem
        >
    > Search(
        [FromQuery(Name = "user")] string userKey,
        [BindRequired] [FromQuery(Name = "q")] string searchString,
        CancellationToken cancellationToken
    )
    {
        if (!await HttpContext.RequestHasAccessToUserKey(userKey, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (string.IsNullOrWhiteSpace(searchString))
        {
            return TypedResults.ValidationProblem([
                new KeyValuePair<string, string[]>(
                    "q",
                    ["search parameter 'q' must not be empty, null, or whitespace."]
                ),
            ]);
        }

        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            FeatureNotEnabledResult,
            BookNotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        > result = await recipeService.SearchRecipesAsync(
            userKey,
            new()
            {
                SearchString = searchString,
                // todo - limit
                // todo - pagination?
            },
            cancellationToken
        );
        return result.Result switch
        {
            Database.Results.NotFoundResult or BookNotFoundResult => TypedResults.NotFound(),
            UserNotFoundResult => TypedResults.Forbid(),
            ValidationFailureResult validation => TypedResults.ValidationProblem(validation.Errors),
            SuccessResult<List<RecipeListEntryDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(r => new RecipeListEntryJsonResponse(r))
            ),
            _ => throw new NotImplementedException(),
        };
    }
}
