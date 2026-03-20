import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useMemo } from "preact/hooks";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import type { IRecipeBookStore } from "../services/recipe-types";

export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: UseGetRecipeBookByIdArgs,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const idForCache = useMemo(() => {
    if (!bookId) {
      return "";
    }
    if (args?.noCache) {
      return `${bookId}?${Date.now()}`;
    }
    return bookId;
  }, [args?.noCache, bookId]);
  return useQuery(
    getRecipeBookByIdQueryArgs(userKey, bookId, idForCache, recipeStore, args),
  );
}

export function getRecipeBookByIdQueryArgs(
  userKey: string | undefined | null,
  bookId: string | undefined | null,
  cacheKey: string,
  recipeStore: IRecipeBookStore,
  args?: UseGetRecipeBookByIdArgs,
) {
  return {
    queryKey: recipeBookByIdCacheKey(cacheKey ?? ""),
    enabled: bookId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : undefined,
    refetchInterval: args?.refetchInterval,
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

/**
 * creates the cache key used for a recipe book by id fetch
 * @param bookId the recipe book id
 * @returns the cache key
 */
export function recipeBookByIdCacheKey(bookId: string) {
  return ["feature:recipes", "getRecipeBookById", bookId];
}

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
