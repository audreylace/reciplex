import { useQuery } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeQueryKeyRoot } from "../utils/recipe-queries/recipe-query-key-factory";

/** args for the `useSearchRecipesInBook` hook which is used to search recipe records in a book */
export interface IUseSearchRecipeInBookArgs {
  /** set to true to disable the query. By default the query is enabled */
  disable?: boolean;
  /** the user search string, if not set or empty then the search is disabled */
  searchString?: string;
  /** the offset into the search listing */
  offset?: number;
  /** number of entries to return in a single search page */
  limit?: number;
  /** the id of the book, if not set, then the search is disabled */
  bookId?: string;
}

/**
 * hook to search for recipes in a book. See `IUseSearchRecipeInBookArgs` for hook arguments.
 * @param args hook arguments
 * @returns query result
 */
export function useSearchRecipesInBook(args?: IUseSearchRecipeInBookArgs) {
  const searchString = (args?.searchString ?? "").trim();
  const disable = !!args?.disable;
  const offset = args?.offset ?? 0;
  const limit = args?.limit ?? 10; // default to 10 results
  const bookId = (args?.bookId ?? "").trim();

  if (offset < 0 || limit < 0) {
    throw new Error("offset and limit, when set, must be 0 or positive");
  }

  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();

  const isEnabled = !!(!disable && searchString && bookId && userKey);
  return useQuery({
    queryKey: searchRecipesQueryKey(userKey ?? "", bookId, {
      limit,
      offset,
      searchString,
    }),
    enabled: isEnabled,
    queryFn: async () => {
      if (!isEnabled) {
        throw Error("query is not enabled");
      }
      return recipeStore.searchRecipes(userKey, searchString, {
        bookId,
        offset,
        limit,
      });
    },
  });
}

/** the cache key branch */
const recipeSearchCacheKeyBranch = "search-recipes";

/**
 * Computes the cache key for a recipe search
 * @param userKey the user requesting the book
 * @param bookKey the key of the book
 * @param cacheArgs the args of the search, used to construct a unique cache key based on different inputs
 * @returns computed query cache key
 */
function searchRecipesQueryKey(
  userKey: string,
  bookKey: string,
  cacheArgs: IUseSearchRecipeInBookCacheArgs,
) {
  return [
    recipeQueryKeyRoot(userKey),
    recipeSearchCacheKeyBranch,
    bookKey,
    cacheArgs,
  ];
}

/** Additional args identifying the cache key of a recipe search inside a book */
interface IUseSearchRecipeInBookCacheArgs {
  /** the supplied search string. If not supplied, set to an empty string. */
  searchString: string;
  /** the search offset. If not supplied then set to 0. */
  offset: number;
  /** the search limit. If not supplied then set to 0. */
  limit: number;
}
