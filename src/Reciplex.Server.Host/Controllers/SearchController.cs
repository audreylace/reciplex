using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Reciplex.Server.Abstractions.StringIdProvider;
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
    /// <param name="ct">async cancellation token associated with the request</param>
    /// <param name="max">max number of hits to return from the search index</param>
    /// <param name="bookIdString">if supplied, limits te search to just this book</param>
    /// <returns>the http result to send back to the caller</returns>
    [HttpGet]
    public async Task<
        Results<Ok<SearchJsonResponse>, NotFound, BadRequest, ValidationProblem>
    > Search(
        [FromQuery(Name = "user")] string userKey,
        [FromQuery(Name = "q")] string? searchString,
        [FromQuery(Name = "limit")] int? maxNullable,
        [FromQuery(Name = "offset")] int? offsetNullable,
        [FromQuery(Name = "book")] List<string>? bookIdsNullable,
        [FromQuery(Name = "kind")] List<string>? documentKindNullable,
        CancellationToken ct
    )
    {
        int max = maxNullable ?? 10;
        int offset = offsetNullable ?? 0;
        if (stringIdProvider.AsLong(userKey) is not long userId || userId < 0)
        {
            return TypedResults.BadRequest();
        }

        if (searchString is not null && searchString.Length > 100)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    { "q", ["Expect 'q' to be no longer then 100 characters"] },
                }
            );
        }

        if (!await HttpContext.RequestHasAccessToUserKey(userId, ct))
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
                    return TypedResults.BadRequest();
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

        DatabaseResultVariant<
            BookNotFoundResult,
            ValidationFailureResult,
            FeatureNotEnabledResult,
            SuccessResult<SearchResultDao>
        > result = await searchIndexFacade.SearchAsync(
            userId,
            new()
            {
                SearchString = searchString ?? "",
                BookIds = booksToTarget,
                Limit = max,
                Offset = offset,
                DocumentKinds = documentKindsToTarget,
            },
            ct
        );
        return result.Result switch
        {
            FeatureNotEnabledResult => TypedResults.NotFound(),
            BookNotFoundResult or ValidationFailureResult => TypedResults.BadRequest(),
            SuccessResult<SearchResultDao> successResult => TypedResults.Ok(
                ToSearchJsonResponse(successResult.Value)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Converts <paramref name="dao"/> to json result
    /// </summary>
    /// <param name="dao">the model to convert to json</param>
    /// <returns>the json result</returns>
    private SearchJsonResponse ToSearchJsonResponse(SearchResultDao dao)
    {
        return new SearchJsonResponse()
        {
            Books =
            [
                .. dao.RecipeBooks.Select(b => new ConciseRecipeBookJsonResponse()
                {
                    BookKey = stringIdProvider.AsString(b.Id),
                    Name = b.Name,
                    ShortDescription = b.ShortDescription,
                }),
            ],
            Recipes =
            [
                .. dao.Recipes.Select(recipe => new RecipeListEntryJsonResponse()
                {
                    RecipeKey = stringIdProvider.AsString(recipe.Id),
                    Name = recipe.Name,
                    ShortDescription = recipe.ShortDescription,
                    BookKey = stringIdProvider.AsString(recipe.BookId),
                }),
            ],
            Hits =
            [
                .. dao.Hits.Select(h => new HitsPtrJson(
                    h.Kind switch
                    {
                        RecipeBookSearchIndexDocumentKind.Recipe => "recipe",
                        RecipeBookSearchIndexDocumentKind.RecipeBook => "book",
                        _ => throw new NotImplementedException(),
                    },
                    stringIdProvider.AsString(h.Id)
                )),
            ],
        };
    }
}
