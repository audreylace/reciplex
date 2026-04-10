import { useMutation } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * Mutation for deleting a recipe
 * @returns delete mutation from useMutation
 */
export function useDeleteRecipeMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (
      {
        id,
        versionTag,
      }: {
        id: string;
        versionTag: string;
      },
      { client },
    ) => {
      await recipeStore.deleteRecipe(AssertString(userKey), id, versionTag);

      // update cache based on new recipe state
      updateRecipeInCache(client, AssertString(userKey), id);
    },
  });
}
