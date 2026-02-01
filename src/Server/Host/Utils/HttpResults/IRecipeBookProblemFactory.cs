using Microsoft.AspNetCore.Http.HttpResults;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.Utils.HttpResults;

public interface IRecipeBookProblemFactory
{
    ProblemHttpResult BookNotFoundResult(RecipeBookKey bookKey);
    ProblemHttpResult BookPreconditionFailed(RecipeBookKey bookKey, string failedHeader);
    ValidationProblem BookValidationProblem(
        RecipeBookKey? bookKey,
        IDictionary<string, string[]>? errors
    );
    ProblemHttpResult OperationOnBookForbidden(RecipeBookKey bookKey);
}
