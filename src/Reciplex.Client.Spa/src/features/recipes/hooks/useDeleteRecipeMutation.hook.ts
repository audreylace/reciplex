import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: ({ id, versionTag }: IUseDeleteRecipeMutationArgs) =>
      recipeStore.deleteRecipe(AssertString(userKey), id, versionTag),
    onSuccess: (_, { id }) =>
      updateRecipeInCache(queryClient, AssertString(userKey), id),
  });
}

interface IUseDeleteRecipeMutationArgs {
  id: string;
  versionTag: string;
}
