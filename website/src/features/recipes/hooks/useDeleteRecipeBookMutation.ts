import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeBookByIdCacheKey } from "./useGetRecipeBookById.hook";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeBookMutation() {
  const queryClient = useQueryClient();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async ({
      bookId,
      versionTag,
    }: {
      bookId: string;
      versionTag: string;
    }) => {
      await recipeStore.deleteRecipeBook(bookId, versionTag);
      queryClient.invalidateQueries({
        queryKey: recipeBookByIdCacheKey(bookId),
      });
    },
  });
}
