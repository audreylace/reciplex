import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type IUpdateRecipeArgs } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * creates a mutation for updating a recipe
 * @returns mutation object
 */
export function useUpdateRecipeMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: (data: IUpdateRecipeArgs & { recipeId: string }) =>
      recipeStore.updateRecipe(AssertString(userKey), data.recipeId, {
        name: data.name,
        details: data.details,
        shortDescription: data.shortDescription,
        versionTag: data.versionTag,
      }),
    onSuccess: (result) =>
      updateRecipeInCache(queryClient, AssertString(userKey), result),
  });
}
