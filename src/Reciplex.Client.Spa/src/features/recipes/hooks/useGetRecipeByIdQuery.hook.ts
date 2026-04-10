import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import type { IRecipeBookStore } from "../services/recipe-types";
import { recipeQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

/**
 * arguments for @see useGetRecipeByIdQuery
 */
export type UseGetRecipeByIdQueryArgs = {
  /**
   * if the query is enabled
   */
  enabled?: boolean;
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

  return useQuery(getRecipeByIdQueryArgs(userKey, recipeId, recipeStore, args));
}

export function getRecipeByIdQueryArgs(
  userKey: string | null | undefined,
  recipeId: string | null | undefined,
  recipeStore: IRecipeBookStore,
  args?: UseGetRecipeByIdQueryArgs,
) {
  return {
    queryKey: recipeQueryKey(userKey ?? "", recipeId ?? ""),
    enabled: recipeId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : 60 * 1000, // todo - hard code this somewhere
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
