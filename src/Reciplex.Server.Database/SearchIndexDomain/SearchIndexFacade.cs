using FluentValidation.Results;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Implements <see cref="ISearchIndexFacade"/>
/// </summary>
/// <param name="searchRepo">repository for running searches</param>
/// <param name="booksService">service for getting books</param>
sealed class SearchIndexFacade(ISearchIndexRepository searchRepo, IRecipeBooksService booksService)
    : ISearchIndexFacade
{
    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            BookNotFoundResult,
            ValidationFailureResult,
            FeatureNotEnabledResult,
            SuccessResult<SearchResultDao>
        >
    > SearchAsync(long userId, SearchIndexAsyncArgs args, CancellationToken ct)
    {
        if (!searchRepo.IsEnabled())
        {
            return new FeatureNotEnabledResult();
        }

        SearchIndexAsyncArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult();
        }

        List<long> bookIds = [];
        if (args.BookIds?.Count > 0)
        {
            foreach (long bookId in args.BookIds)
            {
                if (!await booksService.HasViewAccessAsync(userId, bookId, ct))
                {
                    return new BookNotFoundResult(bookId);
                }

                bookIds.Add(bookId);
            }
        }
        else
        {
            bookIds = await booksService.ListBooksByIdAsync(userId, args.MaxBooks, ct);
        }

        if (bookIds.Count < 1)
        {
            return new SuccessResult<SearchResultDao>(
                new()
                {
                    Hits = [],
                    Recipes = [],
                    RecipeBooks = [],
                }
            );
        }

        List<RecipeBookSearchIndexDocument> matchesFromSearchIndex =
            await searchRepo.SearchForDocumentsAsync(
                new()
                {
                    SearchString = args.SearchString,
                    BookIds = bookIds,
                    Limit = args.Limit,
                    Offset = args.Offset,
                    LimitToKind = args.DocumentKinds,
                },
                ct
            );

        SearchResultDao searchResultDao = new();

        // pre-populate the cache index to avoid duplicate book entries.
        // Where possible, we use the data the search index gave us already instead of hitting
        // the database.
        Dictionary<long, bool> bookLookupCache = matchesFromSearchIndex
            .Where(m => m.Kind == RecipeBookSearchIndexDocumentKind.RecipeBook)
            .ToDictionary(b => b.RecipeBookId, b => true);

        foreach (var match in matchesFromSearchIndex)
        {
            if (
                match.Kind == RecipeBookSearchIndexDocumentKind.Recipe
                && match.RecipeId is long recipeId
            )
            {
                if (!bookLookupCache.TryGetValue(match.RecipeBookId, out bool lookupResult))
                {
                    DatabaseResultVariant<
                        SuccessResult<RecipeBookConciseDao>,
                        NotFoundResult
                    > dbResult = await booksService.GetRecipeBookConciseAsync(
                        match.RecipeBookId,
                        userId,
                        ct
                    );
                    if (dbResult.Result is SuccessResult<RecipeBookConciseDao> successResult)
                    {
                        searchResultDao.RecipeBooks.Add(successResult.Value);
                        lookupResult = true;
                    }

                    bookLookupCache[match.RecipeBookId] = lookupResult;
                }

                if (lookupResult)
                {
                    searchResultDao.Recipes.Add(
                        new()
                        {
                            Id = recipeId,
                            Name = match.Name,
                            ShortDescription = match.ShortDescription,
                            BookId = match.RecipeBookId,
                        }
                    );
                    searchResultDao.Hits.Add(
                        new SearchIdPointer(RecipeBookSearchIndexDocumentKind.Recipe, recipeId)
                    );
                }
            }
            else if (match.Kind == RecipeBookSearchIndexDocumentKind.RecipeBook)
            {
                // bookLookupCache is already pre-populated with this book
                // id at this point so we don't need to mutate it.
                //
                searchResultDao.RecipeBooks.Add(
                    new()
                    {
                        Id = match.RecipeBookId,
                        Name = match.Name,
                        ShortDescription = match.ShortDescription,
                    }
                );
                searchResultDao.Hits.Add(
                    new SearchIdPointer(
                        RecipeBookSearchIndexDocumentKind.RecipeBook,
                        match.RecipeBookId
                    )
                );
            }
        }

        return new SuccessResult<SearchResultDao>(searchResultDao);
    }
}
