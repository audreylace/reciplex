import { useMutation } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeArgs } from "../services/recipe-types";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { updateRecipeInCache } from "../utils/recipe-queries/recipe-query-helpers";

export function useCreateRecipeMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (data: ICreateRecipeArgs, { client }) => {
      const result = await recipeStore.createRecipe(
        AssertString(userKey),
        data,
      );
      // update cache based on new recipe state
      updateRecipeInCache(client, AssertString(userKey), result, true);
      return result;
    },
  });
}
