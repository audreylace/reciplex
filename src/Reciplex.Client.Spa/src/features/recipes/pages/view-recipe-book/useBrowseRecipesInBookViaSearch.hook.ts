import { useSearchParams } from "react-router";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import { useSearchRecipesInBook } from "../../hooks/useSearchRecipesInBook.hook";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";

/** name of the search query parameter */
const searchQueryName = "q";

/**
 * returns the search string
 */
export function useSearchString() {
  return (useSearchParams()[0].get(searchQueryName) ?? "").trim();
}

/** returns a method that can be used to set the search query parameter */
export function useSetSearchQuery() {
  const setSearchParams = useSearchParams()[1];
  return (s: string) => {
    s = s.trim();
    if (s) {
      setSearchParams({ q: s });
    } else {
      setSearchParams({});
    }
  };
}

/**
 * Holds logic for searching
 */
export function useBrowseRecipesInBookViaSearch({
  bookId,
  searchValue,
}: IUseBrowseRecipesInBookViaSearchArgs): IUseBrowseRecipesInBookViaSearchReturn {
  const selectedSize = useRecipeClientStateContext(
    (state) => state.recipeListPageSize,
  );
  const searchQuery = useSearchRecipesInBook({
    searchString: searchValue,
    limit: selectedSize,
    bookId: bookId,
  });

  return {};
}

/** args for the hook `useBrowseRecipesInBookViaSearch` */
export interface IUseBrowseRecipesInBookViaSearchArgs {
  /** if the hook is enabled */
  enable: boolean;
  /** the id of the book */
  bookId: string;
  /** search string */
  searchValue: string;
}

/**
 * return for `useBrowseRecipesInBookViaSearch`
 */
export interface IUseBrowseRecipesInBookViaSearchReturn {
  /** data for the current page */
  data?: IRecipeListEntryJsonResponse[];
  /** if data for the current page has loaded in */
  dataSuccess: boolean;
  /** if data for the current page is still loading */
  dataPending: boolean;
  /** if the data load failed */
  dataError: boolean;
}
