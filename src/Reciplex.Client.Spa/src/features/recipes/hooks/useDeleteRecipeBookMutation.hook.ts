import { useMutation } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeBookInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeBookMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (
      {
        bookId,
        versionTag,
      }: {
        bookId: string;
        versionTag: string;
      },
      { client },
    ) => {
      await recipeStore.deleteRecipeBook(
        AssertString(userKey),
        bookId,
        versionTag,
      );
      updateRecipeBookInCache(client, AssertString(userKey), bookId);
    },
  });
}
