using FluentValidation.Results;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchIndexDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Implements <see cref="ISearchIndexFacade"/>
/// </summary>
/// <param name="searchRepo">repository for running searches</param>
/// <param name="booksService">service for getting bools</param>
/// <param name="recipesService">service for getting recipes</param>
sealed class SearchIndexFacade(
    ISearchIndexRepository searchRepo,
    IRecipeBooksService booksService,
    IRecipesService recipesService
) : ISearchIndexFacade
{
    public async Task<
        DatabaseResultVariant<
            BookNotFoundResult,
            ValidationFailureResult,
            FeatureNotEnabledResult,
            SuccessResult<SearchResultDao>
        >
    > SearchAsync(long userId, SearchIndexAsyncArgs args, CancellationToken ct)
    {
        if (searchRepo.IsEnabled())
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

        List<SearchIndexDocumentPointer> matchesFromSearchIndex =
            await searchRepo.SearchForDocumentPointersAsync(
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
        Dictionary<long, bool> bookLookupResult = [];
        foreach (var match in matchesFromSearchIndex)
        {
            if (
                match.Kind == RecipeBookSearchIndexDocumentKind.Recipe
                && match is SearchIndexRecipeDocumentPointers recipeDocumentPtr
            )
            {
                if (
                    await CheckAndLoadBookAsync(
                        userId,
                        searchResultDao,
                        bookLookupResult,
                        recipeDocumentPtr.BookId,
                        ct
                    )
                )
                {
                    var recipeResult = await recipesService.GetRecipeAsync(
                        recipeDocumentPtr.RecipeId,
                        userId,
                        ct
                    );
                    if (recipeResult.Result is SuccessResult<RecipeDao> recipe)
                    {
                        searchResultDao.Recipes.Add(recipe.Value);
                        searchResultDao.Hits.Add(
                            new SearchIdPointer(
                                RecipeBookSearchIndexDocumentKind.Recipe,
                                recipeDocumentPtr.RecipeId
                            )
                        );
                    }
                }
            }
            else if (
                match.Kind == RecipeBookSearchIndexDocumentKind.RecipeBook
                && match is SearchIndexBookDocumentPointers bookDocPtr
            )
            {
                if (
                    await CheckAndLoadBookAsync(
                        userId,
                        searchResultDao,
                        bookLookupResult,
                        bookDocPtr.BookId,
                        ct
                    )
                )
                {
                    searchResultDao.Hits.Add(
                        new SearchIdPointer(
                            RecipeBookSearchIndexDocumentKind.RecipeBook,
                            bookDocPtr.BookId
                        )
                    );
                }
            }
        }

        return new SuccessResult<SearchResultDao>(searchResultDao);
    }

    private async Task<bool> CheckAndLoadBookAsync(
        long userId,
        SearchResultDao searchResultDao,
        Dictionary<long, bool> bookLookupResult,
        long bookId,
        CancellationToken ct
    )
    {
        if (bookLookupResult.TryGetValue(bookId, out bool cachedResult))
        {
            if (!cachedResult)
            {
                return false;
            }
        }
        else
        {
            var dbResult = await booksService.GetRecipeBookAsync(bookId, userId, ct);
            if (dbResult.Result is SuccessResult<RecipeBookDao> successResult)
            {
                searchResultDao.RecipeBooks.Add(successResult.Value);
                bookLookupResult[bookId] = true;
            }
            else
            {
                bookLookupResult[bookId] = false;
                return false;
            }
        }

        return true;
    }
}
