import { useMutation } from "@tanstack/react-query";
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
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (
      data: IUpdateRecipeArgs & { recipeId: string },
      { client },
    ) => {
      const result = await recipeStore.updateRecipe(
        AssertString(userKey),
        data.recipeId,
        {
          name: data.name,
          details: data.details,
          shortDescription: data.shortDescription,
          versionTag: data.versionTag,
        },
      );

      // update cache based on new recipe state
      updateRecipeInCache(client, AssertString(userKey), result);
      return result;
    },
  });
}
