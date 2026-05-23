import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { recipeQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";
import { useMemo } from "react";

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
  const enabled = !!recipeId && !!userKey && (args?.enabled ?? true);

  return useQuery({
    queryKey: recipeQueryKey(userKey ?? "", recipeId ?? ""),
    refetchOnMount: args?.alwaysFresh ? "always" : true,
    refetchOnWindowFocus: args?.alwaysFresh ? "always" : true,
    enabled: enabled,
    queryFn: async () => {
      if (!recipeId || !userKey) {
        throw Error("invalid recipe id");
      }

      return recipeStore.getRecipeById(userKey, recipeId, {
        noCache: args?.alwaysFresh,
      });
    },
  });
}

export function useGetRecipeByIdQueryKey(recipeId: string) {
  const userKey = useActiveUserKey() ?? "";
  const normalizedRecipeId = recipeId ?? "";

  return useMemo(
    () => recipeQueryKey(userKey, normalizedRecipeId),
    [normalizedRecipeId, userKey],
  );
}

/**
 * arguments for @see useGetRecipeByIdQuery
 */
export type UseGetRecipeByIdQueryArgs = {
  /**
   * if the query is enabled
   */
  enabled?: boolean;
  /**
   * Data will refetch on mount even if not stale.
   * Client will call remote bypassing the browsers cache.
   */
  alwaysFresh?: boolean;
};
