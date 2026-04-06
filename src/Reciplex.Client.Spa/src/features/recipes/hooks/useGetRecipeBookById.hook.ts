import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import type { IRecipeBookStore } from "../services/recipe-types";
import { useMemo } from "preact/hooks";

export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: UseGetRecipeBookByIdArgs,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useQuery(
    getRecipeBookByIdQueryArgs(userKey, bookId, recipeStore, args),
  );
}

export function getRecipeBookByIdQueryArgs(
  userKey: string | undefined | null,
  bookId: string | undefined | null,
  recipeStore: IRecipeBookStore,
  args?: UseGetRecipeBookByIdArgs,
) {
  return {
    queryKey: getRecipeBookByIdCacheKey(userKey ?? "", bookId ?? ""),
    staleTime: args?.noCache ? 0 : undefined,
    gcTime: args?.noCache ? 0 : undefined,
    queryFn: async () => {
      if (!bookId || !userKey) {
        throw Error("need a recipe book id");
      }

      return await recipeStore.getRecipeBook(userKey, bookId, {
        noCache: args?.noCache,
      });
    },
  };
}

export function useGetRecipeBookByIdCacheKey(
  bookId: string | null | undefined,
) {
  const userKey = useActiveUserKey();
  return useMemo(
    () => getRecipeBookByIdCacheKey(userKey ?? "", bookId ?? ""),
    [bookId, userKey],
  );
}

/**
 * creates the cache key used for a recipe book by id fetch
 * @param userKey the user requesting the book
 * @param bookId the recipe book id
 * @returns the cache key
 */
export function getRecipeBookByIdCacheKey(userKey: string, bookId: string) {
  return ["feature:recipes", "getRecipeBookById", userKey, bookId];
}

/**
 * arguments for @see useGetRecipeBookById
 */
export type UseGetRecipeBookByIdArgs = {
  /**
   * When true, data will be loaded directly from the remote
   */
  noCache?: boolean;
};
