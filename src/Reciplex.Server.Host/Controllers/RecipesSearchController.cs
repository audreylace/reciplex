using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
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
public class RecipesSearchController(
    IRecipesService recipeService,
    IStringIdProvider stringIdProvider
) : ControllerBase
{
    [HttpGet]
    public async Task<
        Results<
            Ok<IEnumerable<RecipeListEntryJsonResponse>>,
            NotFound,
            ForbidHttpResult,
            BadRequest
        >
    > Search(
        [FromQuery(Name = "user")] string userKey,
        [BindRequired] [FromQuery(Name = "q")] string searchString,
        CancellationToken cancellationToken
    )
    {
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.Forbid();
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        if (string.IsNullOrWhiteSpace(searchString))
        {
            return TypedResults.BadRequest();
        }

        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            FeatureNotEnabledResult,
            BookNotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        > result = await recipeService.SearchRecipesAsync(
            userId,
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
            ValidationFailureResult validation => TypedResults.BadRequest(),
            SuccessResult<List<RecipeListEntryDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(ToListEntryJson)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a recipe json model from a <see cref="RecipeListEntryDao"/>
    /// </summary>
    /// <param name="recipe">the source recipe data used to populate this model</param>
    public RecipeListEntryJsonResponse ToListEntryJson(RecipeListEntryDao recipe)
    {
        return new RecipeListEntryJsonResponse()
        {
            RecipeKey = stringIdProvider.AsString(recipe.Id),
            Name = recipe.Name,
            ShortDescription = recipe.ShortDescription,
            BookKey = stringIdProvider.AsString(recipe.BookId),
        };
    }
}
