import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useMemo } from "preact/hooks";
import { recipeBookQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: UseGetRecipeBookByIdArgs,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();

  return useQuery({
    queryKey: recipeBookQueryKey(userKey ?? "", bookId ?? ""),
    staleTime: args?.noCache ? 0 : 60 * 1000, // todo - hard code this somewhere
    queryFn: async () => {
      if (!bookId || !userKey) {
        throw Error("need a recipe book id");
      }

      return await recipeStore.getRecipeBook(userKey, bookId, {
        noCache: args?.noCache,
      });
    },
  });
}

export function useGetRecipeBookByIdCacheKey(
  bookId: string | null | undefined,
) {
  const userKey = useActiveUserKey();
  return useMemo(
    () => recipeBookQueryKey(userKey ?? "", bookId ?? ""),
    [bookId, userKey],
  );
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
