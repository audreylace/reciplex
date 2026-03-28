import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeMutation() {
  const userKey = useActiveUserKey();
  const queryClient = useQueryClient();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async ({
      id,
      versionTag,
    }: {
      id: string;
      versionTag: string;
    }) => {
      await recipeStore.deleteRecipe(AssertString(userKey), id, versionTag);
      queryClient.invalidateQueries({ queryKey: recipeByIdCacheKey(id) });
    },
  });
}
