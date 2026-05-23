import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { recipeBookSharedAccessQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

/**
 * mutation for requesting access to a recipe book
 */
export function useRequestAccessToBookMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: ({ shareKey, bookId }: { shareKey: string; bookId: string }) =>
      recipeStore.postRecipeBookAccessRequest(
        AssertString(userKey),
        bookId,
        shareKey,
      ),
    onSuccess: (newData, oldData) => {
      queryClient.resetQueries({
        queryKey: recipeBookSharedAccessQueryKey(
          AssertString(userKey),
          oldData.bookId,
          oldData.shareKey,
        ),
      });
      queryClient.resetQueries({
        queryKey: recipeBookSharedAccessQueryKey(
          AssertString(userKey),
          oldData.bookId,
        ),
      });

      if (newData) {
        queryClient.setQueryData(
          recipeBookSharedAccessQueryKey(
            AssertString(userKey),
            newData.bookKey,
          ),
          newData,
        );
        queryClient.setQueryData(
          recipeBookSharedAccessQueryKey(
            AssertString(userKey),
            newData.bookKey,
            oldData.shareKey,
          ),
          newData,
        );
      }
    },
  });
}
