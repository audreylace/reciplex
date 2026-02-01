using Microsoft.AspNetCore.Http.HttpResults;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Utils.HttpResults;

public interface IRecipeProblemFactory
{
    /// <summary>
    /// Returns a validation problem scoped to a recipe
    /// </summary>
    /// <param name="recipeId">The recipe ID</param>
    /// <param name="errors">Map of validation errors</param>
    /// <returns>The validation problem</returns>
    public ValidationProblem RecipeValidationProblem(
        RecipeKey? recipeId,
        IDictionary<string, string[]>? errors
    );

    /// <summary>
    /// Generates a 412 pre-condition failure
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="failedHeader">The header that triggered the failure</param>
    /// <returns>the 412 result</returns>
    public ProblemHttpResult RecipePreconditionFailed(RecipeKey recipeId, string failedHeader);

    /// <summary>
    /// Generates a 403 forbidden error
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <returns>the 404 result</returns>
    public ProblemHttpResult OperationOnRecipeForbidden(RecipeKey recipeId);

    /// <summary>
    /// Generates a 404 error
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <returns>the 404 result</returns>
    public ProblemHttpResult RecipeNotFoundResult(RecipeKey recipeId);
}
