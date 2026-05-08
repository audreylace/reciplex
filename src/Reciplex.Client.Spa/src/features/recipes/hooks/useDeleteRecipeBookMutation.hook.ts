import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeBookInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * Mutation for deleting a recipe book
 */
export function useDeleteRecipeBookMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: ({ bookId, versionTag }: IUseDeleteRecipeBookMutationArgs) =>
      recipeStore.deleteRecipeBook(AssertString(userKey), bookId, versionTag),
    onSuccess: (_, { bookId }) =>
      updateRecipeBookInCache(queryClient, AssertString(userKey), bookId),
  });
}

interface IUseDeleteRecipeBookMutationArgs {
  bookId: string;
  versionTag: string;
}
