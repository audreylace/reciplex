using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Models;

namespace Reciplex.Server.Host.Controllers;

/// <summary>
/// API for searching recipe books
/// </summary>
/// <param name="bookService">service for getting books</param>
/// <param name="stringIdProvider">service for marshaling longs to and from strings</param>
[ApiController]
[Route("search/books")]
[Authorize]
public class RecipeBooksSearchController(
    IRecipeBooksService bookService,
    IStringIdProvider stringIdProvider
) : ControllerBase
{
    /// <summary>
    /// Searches for books
    /// </summary>
    /// <param name="userKey">the key of the acting user</param>
    /// <param name="searchString">the query string</param>
    /// <param name="cancellationToken">async cancellation token associated with the request</param>
    /// <param name="max">max number of hits to return from the search index</param>
    /// <returns>the http result to send back to the caller</returns>
    [HttpGet]
    public async Task<
        Results<Ok<IEnumerable<RecipeBookJsonResponse>>, NotFound, BadRequest, ValidationProblem>
    > Search(
        [FromQuery(Name = "user")] string userKey,
        [BindRequired] [FromQuery(Name = "q")] string searchString,
        [FromQuery(Name = "max")] int? maxNullable,
        [FromQuery(Name = "offset")] int? offsetNullable,
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

        DatabaseResultVariant<
            SuccessResult<List<RecipeBookDao>>,
            FeatureNotEnabledResult,
            UserNotFoundResult,
            ValidationFailureResult
        > result = await bookService.SearchBooksAsync(
            userId,
            new()
            {
                SearchString = searchString,
                Limit = max,
                Offset = offset,
            },
            cancellationToken
        );
        return result.Result switch
        {
            FeatureNotEnabledResult => TypedResults.NotFound(),
            UserNotFoundResult or ValidationFailureResult => TypedResults.BadRequest(),
            SuccessResult<List<RecipeBookDao>> successResult => TypedResults.Ok(
                successResult.Value.Select(ToJson)
            ),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Builds an instance of <see cref="RecipeBookJsonResponse"/>
    /// </summary>
    /// <param name="book">the dao data to convert</param>
    private RecipeBookJsonResponse ToJson(RecipeBookDao book)
    {
        return new RecipeBookJsonResponse()
        {
            BookKey = stringIdProvider.AsString(book.Id),
            Name = book.Name,
            ShortDescription = book.ShortDescription,
            OwningUserKey = stringIdProvider.AsString(book.OwningUserId),
            Created = book.Created,
            LastModified = book.LastModified,
            ConcurrencyTag = book.ConcurrencyTag,
            MayEdit = book.MayEditBook,
            MayDelete = book.MayDeleteBook,
            MayShare = book.MayShareBook,
            MayManageAccess = book.MayManageAccess,
            ShareKey = book.MayShareBook ? book.ShareKey : null,
        };
    }
}
