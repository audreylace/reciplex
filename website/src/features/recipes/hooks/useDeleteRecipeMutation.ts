import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeMutation() {
  const queryClient = useQueryClient();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (id: string) => {
      await recipeStore.deleteRecipe(id);
      queryClient.invalidateQueries({ queryKey: recipeByIdCacheKey(id) });
    },
  });
}
