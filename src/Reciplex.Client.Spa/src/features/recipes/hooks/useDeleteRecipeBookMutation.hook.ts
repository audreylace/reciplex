import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { getRecipeBookByIdCacheKey } from "./useGetRecipeBookById.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeBookMutation() {
  const userKey = useActiveUserKey();
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
      await recipeStore.deleteRecipeBook(
        AssertString(userKey),
        bookId,
        versionTag,
      );
      queryClient.invalidateQueries({
        queryKey: getRecipeBookByIdCacheKey(AssertString(userKey), bookId),
      });
    },
  });
}
