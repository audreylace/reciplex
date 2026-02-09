import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeArgs } from "../../../services/recipe-store";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";

export function useCreateRecipeMutation() {
  const queryClient = useQueryClient();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (data: ICreateRecipeArgs) => {
      const result = await recipeStore.createRecipe(data);
      queryClient.setQueryData(recipeByIdCacheKey(result.recipe.id), result);
      return result;
    },
  });
}
