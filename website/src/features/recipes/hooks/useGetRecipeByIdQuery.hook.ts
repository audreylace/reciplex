import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";

/**
 * arguments for @see useGetRecipeByIdQuery
 */
export type UseGetRecipeByIdQueryArgs = {
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

/**
 * gets a recipe by id
 * @param recipeId the ID of the recipe
 * @param enabled optional flag to control if the query is active
 * @returns result from useQuery hook
 */
export function useGetRecipeByIdQuery(
  recipeId: string | null | undefined,
  args?: UseGetRecipeByIdQueryArgs,
) {
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: recipeByIdCacheKey(recipeId ?? ""),
    enabled: !!recipeId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : undefined,
    refetchInterval: args?.refetchInterval,
    queryFn: async () => {
      if (!recipeId) {
        throw Error("invalid recipe id");
      }

      return recipeStore.getRecipeById(recipeId, { noCache: args?.noCache });
    },
  });
}

/**
 * creates the cache key used for a recipe by id fetch
 * @param recipeId the recipe id
 * @returns the cache key
 */
export function recipeByIdCacheKey(recipeId: string) {
  return ["feature:recipes", "recipeById", recipeId];
}
