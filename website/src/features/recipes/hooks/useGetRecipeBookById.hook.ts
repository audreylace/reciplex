import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";

/**
 * arguments for @see useGetRecipeBookById
 */
export type UseGetRecipeBookByIdArgs = {
  /**
   * if the query is enabled
   */
  enabled?: boolean;
  /**
   * sets the fetch interval behavior
   */
  refetchInterval?: number | false;
  /**
   * When true, data will be loaded directly from the remote
   */
  noCache?: boolean;
};
export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: UseGetRecipeBookByIdArgs,
) {
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: recipeBookByIdCacheKey(bookId ?? ""),
    enabled: bookId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : undefined,
    refetchInterval: args?.refetchInterval,
    queryFn: async () => {
      if (!bookId) {
        throw Error("need a recipe book id");
      }

      return await recipeStore.getRecipeBook(bookId, {
        noCache: args?.noCache,
      });
    },
  });
}

/**
 * creates the cache key used for a recipe book by id fetch
 * @param bookId the recipe book id
 * @returns the cache key
 */
export function recipeBookByIdCacheKey(bookId: string) {
  return ["feature:recipes", "getRecipeBookById", bookId];
}
