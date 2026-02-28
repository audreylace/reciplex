import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeBookArgs } from "../services/recipe-types";
import { recipeBookByIdCacheKey } from "./useGetRecipeBookById.hook";

/**
 * hook for create recipe book mutation
 * @returns mutation for creating a new recipe book
 */
export function useCreateRecipeBookMutation() {
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (args: ICreateRecipeBookArgs) => {
      const result = await recipeStore.createRecipeBook(args);
      queryClient.setQueryData(recipeBookByIdCacheKey(result.id), result);
      return result;
    },
  });
}
