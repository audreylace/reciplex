import { useQuery } from "@tanstack/react-query";
import { useContext } from "preact/hooks";
import { RecipeStore } from "../../recipes/hooks/useRecipeStoreContext.hook";

/**
 * args for @see useGetUserById
 */
export type UseGetUserByIdArgs = {
  /**
   * if the fetch is enabled
   */
  enabled?: boolean;
};

/**
 * hook for fetching user data
 * @param userId the id to fetch
 * @param args hook args
 * @returns query data from `useQuery`
 */
export function useGetUserById(
  userId: string | undefined | null,
  args?: UseGetUserByIdArgs,
) {
  const recipeStore = useContext(RecipeStore);
  return useQuery({
    queryKey: userByIdCacheKey(userId ?? ""),
    enabled: !!userId ? args?.enabled : false,
    staleTime: 60 * 1000 * 60, // 1 hour
    queryFn: async () => {
      if (!recipeStore) {
        throw Error("need a recipe store");
      }

      if (!userId) {
        throw Error("need a user id");
      }

      return await recipeStore.getUserById(userId);
    },
  });
}

/**
 * creates the cache key used for a user by id fetch
 * @param userId the user id
 * @returns the cache key
 */
export function userByIdCacheKey(userId: string) {
  return ["feature:users", "getUserById", userId];
}
