import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type IUpdateRecipeBookArgs } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeBookByIdCacheKey } from "./useGetRecipeBookById.hook";

/**
 * creates a mutation for updating a recipe book
 * @returns mutation object
 */
export function useUpdateRecipeBookMutation() {
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (
      data: IUpdateRecipeBookArgs & { recipeBookId: string },
    ) => {
      const newData = await recipeStore.updateRecipeBook(data.recipeBookId, {
        name: data.name,
        versionTag: data.versionTag,
        shortDescription: data.shortDescription,
      });
      queryClient.setQueryData(
        recipeBookByIdCacheKey(data.recipeBookId),
        newData,
      );
      return newData;
    },
  });
}
