import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { recipeBookSharedAccessQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

/**
 * mutation for deleting a access request submitted against a recipe book
 */
export function useDeleteAccessToBookMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: ({ bookId }: { bookId: string; shareKey?: string }) =>
      recipeStore.deleteRecipeBookAccessRequest(AssertString(userKey), bookId),
    onSuccess: (_, oldData) => {
      if (oldData.shareKey) {
        queryClient.resetQueries({
          queryKey: recipeBookSharedAccessQueryKey(
            AssertString(userKey),
            oldData.bookId,
            oldData.shareKey,
          ),
        });
      }
      queryClient.resetQueries({
        queryKey: recipeBookSharedAccessQueryKey(
          AssertString(userKey),
          oldData.bookId,
        ),
      });
    },
  });
}
