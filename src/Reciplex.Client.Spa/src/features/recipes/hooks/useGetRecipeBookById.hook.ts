import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useMemo } from "react";
import { recipeBookQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

export function useGetRecipeBookById(
  bookId: string | null | undefined,
  args?: IUseGetRecipeBookByIdArgs,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const enabled = !!(bookId && userKey) && args?.enabled !== false;

  return useQuery({
    queryKey: recipeBookQueryKey(userKey ?? "", bookId ?? ""),
    refetchOnMount: args?.alwaysFresh ? "always" : true,
    refetchOnWindowFocus: args?.alwaysFresh ? "always" : true,
    enabled: enabled,
    queryFn: async () => {
      if (!bookId || !userKey) {
        throw Error("need a recipe book id");
      }

      return recipeStore.getRecipeBook(userKey, bookId, {
        noCache: args?.alwaysFresh,
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

export interface IUseGetRecipeBookByIdArgs {
  alwaysFresh?: boolean;
  enabled?: boolean;
}
