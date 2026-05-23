import { useQuery } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeBookAccessQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

/**
 * hook to get the list of users with access to the recipe book
 * @param bookId the book id to load
 * @param args additional args to change the request
 */
export function useGetUsersWithBookAccess(
  bookId?: string,
  args?: IUseGetUsersWithBookAccess,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();

  return useQuery({
    queryKey: recipeBookAccessQueryKey(userKey ?? "", bookId ?? ""),
    enabled: !!bookId && !!userKey && args?.enabled !== false,
    refetchOnMount: args?.alwaysFresh ? "always" : true,
    refetchOnWindowFocus: args?.alwaysFresh ? "always" : true,
    queryFn: async () => {
      if (!userKey || !bookId) {
        return null;
      }
      return await recipeStore.getUsersWithBookAccess(userKey, bookId, {
        noCache: args?.alwaysFresh,
      });
    },
  });
}

/**
 * arguments for @see useGetUsersWithBookAccess
 */
export type IUseGetUsersWithBookAccess = {
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
