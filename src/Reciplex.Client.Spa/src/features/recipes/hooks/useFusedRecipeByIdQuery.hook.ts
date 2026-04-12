import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useMemo } from "preact/hooks";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { getRecipeByIdQueryArgs } from "./useGetRecipeByIdQuery.hook";
//import { getRecipeBookByIdQueryArgs } from "./useGetRecipeBookById.hook";

/**
 * @obsolete
 */
export function useFusedRecipeByIdQuery(
  recipeId: string | null | undefined,
  args?: IUseFusedRecipeByIdQueryArgs,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();

  const [idForCache, keySuffix] = useMemo(() => {
    if (!recipeId) {
      return ["", ""];
    }
    if (args?.noCache) {
      const dateNow = Date.now().toString();
      return [`${recipeId}?${dateNow}`, dateNow];
    }
    return [recipeId, ""];
  }, [args?.noCache, recipeId]);

  return useQuery({
    queryKey: fusedRecipeByIdCacheKey(idForCache ?? ""),
    enabled: recipeId ? args?.enabled : false,
    staleTime: args?.noCache ? 0 : undefined,
    refetchInterval: args?.refetchInterval,
    queryFn: async ({ client }) => {
      throw Error();
      // if (!recipeId || !userKey) {
      //   throw Error("invalid recipe id");
      // }
      // const recipe = await client.fetchQuery(
      //   getRecipeByIdQueryArgs(userKey, recipeId, idForCache, recipeStore, {
      //     noCache: args?.noCache,
      //     refetchInterval: args?.refetchInterval,
      //     enabled: true,
      //   }),
      // );
      // if (!recipe) {
      //   return null;
      // }
      // const book = await client.fetchQuery(
      //   getRecipeBookByIdQueryArgs(
      //     userKey,
      //     recipe.bookId,
      //     `${recipe.bookId}${keySuffix}`,
      //     recipeStore,
      //     {
      //       noCache: args?.noCache,
      //       refetchInterval: args?.refetchInterval,
      //       enabled: true,
      //     },
      //   ),
      // );
      // if (!book) {
      //   return null;
      // }
      // return { book, recipe };
    },
  });
}

/**
 * creates the cache key used for a fused recipe by id fetch
 * @param recipeId the recipe id
 * @returns the cache key
 */
export function fusedRecipeByIdCacheKey(recipeId: string) {
  return ["feature:recipes", "fusedRecipeById", recipeId];
}

/**
 * arguments for @see useFusedRecipeByIdQuery
 */
export interface IUseFusedRecipeByIdQueryArgs {
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
}
