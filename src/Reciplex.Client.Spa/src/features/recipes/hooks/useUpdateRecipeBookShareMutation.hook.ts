import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { updateRecipeBookInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * creates a mutation for manipulating a recipe book's share link
 * @returns mutation object
 */
export function useUpdateRecipeBookShareKeyMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: ({
      bookId,
      kind,
      versionTag,
    }: IUseUpdateRecipeBooksShareKeyMutationArgs) =>
      recipeStore.updateRecipeBookShareKey(
        AssertString(userKey),
        bookId,
        kind,
        versionTag,
      ),
    onSuccess: (newData) =>
      updateRecipeBookInCache(queryClient, AssertString(userKey), newData),
  });
}

/** args for the `useUpdateRecipeBookShareKeyMutation` mutation */
export interface IUseUpdateRecipeBooksShareKeyMutationArgs {
  /** the id of the book to mutate */
  bookId: string;
  /** how to mutate the share key */
  kind: "regenerate" | "clear";
  /** the version tag for opportunistic concurrency */
  versionTag: string;
}
