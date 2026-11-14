using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;
using Reciplex.Server.SearchExporter;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// API for searching recipes and books
/// </summary>
/// <param name="searchIndexFacade">facade for running the search</param>
/// <param name="stringIdProvider">service for marshaling longs to and from strings</param>
[ApiController]
[Route("search")]
[Authorize]
public class SearchController(
    ISearchIndexFacade searchIndexFacade,
    IStringIdProvider stringIdProvider
) : ControllerBase
{
    /// <summary>
    /// Searches for recipes
    /// </summary>
    /// <param name="userKey">the key of the acting user</param>
    /// <param name="searchString">the query string</param>
    /// <param name="cancellationToken">async cancellation token associated with the request</param>
    /// <param name="max">max number of hits to return from the search index</param>
    /// <param name="bookIdString">if supplied, limits te search to just this book</param>
    /// <returns>the http result to send back to the caller</returns>
    [HttpGet]
    public async Task<
        Results<
            Ok<IEnumerable<RecipeListEntryJsonResponse>>,
            NotFound,
            BadRequest,
            ValidationProblem
        >
    > Search(
        [FromQuery(Name = "user")] string userKey,
        [BindRequired] [FromQuery(Name = "q")] string searchString,
        [FromQuery(Name = "limit")] int? maxNullable,
        [FromQuery(Name = "offset")] int? offsetNullable,
        [FromQuery(Name = "book")] List<string>? bookIdsNullable,
        [FromQuery(Name = "kind")] List<string>? documentKindNullable,
        CancellationToken cancellationToken
    )
    {
        int max = maxNullable ?? 10;
        int offset = offsetNullable ?? 0;
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(searchString))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "q", ["Expect 'q' to have a non-empty value"] },
                }
            );
        }

        if (searchString.Length > 100)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "q", ["Expect 'q' to be no longer then 100 characters"] },
                }
            );
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, cancellationToken))
        {
            return TypedResults.BadRequest();
        }

        if (max < 1 || max > 100)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "max", ["Expect 'max' to be at least 1 and no larger than 100"] },
                }
            );
        }

        if (offset < 0 || offset > 1000)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "offset", ["Expect 'offset' to be at least 0 and no larger than 1000"] },
                }
            );
        }

        List<long> booksToTarget = [];
        if (bookIdsNullable is List<string> bookIdKeys)
        {
            foreach (string bookIdKey in bookIdKeys)
            {
                long? bookId = stringIdProvider.AsLong(bookIdKey);
                if (bookId is null || bookId < 0)
                {
                    return TypedResults.NotFound();
                }

                booksToTarget.Add(bookId.Value);
            }
        }

        List<RecipeBookSearchIndexDocumentKind> documentKindsToTarget = [];
        if (documentKindNullable is List<string> documentKindsStr)
        {
            foreach (string documentKindTargetStr in documentKindsStr)
            {
                switch (documentKindTargetStr)
                {
                    case "recipe":
                        documentKindsToTarget.Add(RecipeBookSearchIndexDocumentKind.Recipe);
                        break;
                    case "book":
                        documentKindsToTarget.Add(RecipeBookSearchIndexDocumentKind.RecipeBook);
                        break;
                    default:
                        return TypedResults.BadRequest();
                }
            }
        }

        var result = await searchIndexFacade.SearchAsync(
            userId,
            new()
            {
                SearchString = searchString,
                BookIds = booksToTarget,
                Limit = max,
                Offset = offset,
                DocumentKinds = documentKindsToTarget,
            },
            cancellationToken
        );
        return result.Result switch
        {
            FeatureNotEnabledResult => TypedResults.NotFound(),
            BookNotFoundResult or UserNotFoundResult or ValidationFailureResult =>
                TypedResults.BadRequest(),
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
    private RecipeListEntryJsonResponse ToListEntryJson(RecipeListEntryDao recipe)
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
