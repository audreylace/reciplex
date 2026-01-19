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
   * ttl of the data
   */
  staleTime?: number;
};
export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: UseGetRecipeBookByIdArgs,
) {
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: recipeBookByIdCacheKey(bookId ?? ""),
    enabled: !!bookId ? args?.enabled : false,
    staleTime: args?.staleTime ?? 60 * 1000,
    refetchInterval: args?.refetchInterval,
    queryFn: async () => {
      if (!bookId) {
        throw Error("need a recipe book id");
      }

      return await recipeStore.getRecipeBook(bookId);
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
