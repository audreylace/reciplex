import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeArgs } from "../services/recipe-types";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { updateRecipeInCache } from "../utils/recipe-queries/recipe-query-helpers";

export function useCreateRecipeMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: (data: ICreateRecipeArgs) =>
      recipeStore.createRecipe(AssertString(userKey), data),
    onSuccess: (result) =>
      updateRecipeInCache(queryClient, AssertString(userKey), result, true),
  });
}
