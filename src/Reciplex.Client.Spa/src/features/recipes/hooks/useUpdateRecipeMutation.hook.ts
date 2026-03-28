import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type IUpdateRecipeArgs } from "../services/recipe-types";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";

/**
 * creates a mutation for updating a recipe
 * @returns mutation object
 */
export function useUpdateRecipeMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (data: IUpdateRecipeArgs & { recipeId: string }) => {
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

      queryClient.setQueryData(recipeByIdCacheKey(result.id), result);
      return result;
    },
  });
}
