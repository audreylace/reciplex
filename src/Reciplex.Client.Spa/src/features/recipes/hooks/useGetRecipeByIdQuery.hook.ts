import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useMemo } from "preact/hooks";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import type { IRecipeBookStore } from "../services/recipe-types";

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
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();

  const idForCache = useMemo(() => {
    if (!recipeId) {
      return "";
    }
    if (args?.noCache) {
      return `${recipeId}?${Date.now()}`;
    }
    return recipeId;
  }, [args?.noCache, recipeId]);

  return useQuery(
    getRecipeByIdQueryArgs(userKey, idForCache, idForCache, recipeStore, args),
  );
}

export function getRecipeByIdQueryArgs(
  userKey: string | null | undefined,
  recipeId: string | null | undefined,
  cacheKey: string,
  recipeStore: IRecipeBookStore,
  args?: UseGetRecipeByIdQueryArgs,
) {
  return {
    queryKey: recipeByIdCacheKey(cacheKey),
    enabled: recipeId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : undefined,
    refetchInterval: args?.refetchInterval,
    queryFn: async () => {
      if (!recipeId || !userKey) {
        throw Error("invalid recipe id");
      }

      return recipeStore.getRecipeById(userKey, recipeId, {
        noCache: args?.noCache,
      });
    },
  };
}

/**
 * creates the cache key used for a recipe by id fetch
 * @param recipeId the recipe id
 * @returns the cache key
 */
export function recipeByIdCacheKey(recipeId: string) {
  return ["feature:recipes", "recipeById", recipeId];
}
