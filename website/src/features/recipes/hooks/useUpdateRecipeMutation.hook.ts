import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type IUpdateRecipeArgs } from "../../../services/recipe-store";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";

/**
 * creates a mutation for updating a recipe
 * @returns mutation object
 */
export function useUpdateRecipeMutation() {
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (data: IUpdateRecipeArgs & { recipeId: string }) => {
      const result = await recipeStore.updateRecipe(data.recipeId, {
        name: data.name,
        details: data.details,
        shortDescription: data.shortDescription,
        versionTag: data.versionTag,
      });

      queryClient.setQueryData(recipeByIdCacheKey(result.recipe.id), result);
      return result;
    },
  });
}
