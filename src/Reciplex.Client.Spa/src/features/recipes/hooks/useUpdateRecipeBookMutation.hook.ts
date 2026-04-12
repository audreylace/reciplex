import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type IUpdateRecipeBookArgs } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { updateRecipeBookInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * creates a mutation for updating a recipe book
 * @returns mutation object
 */
export function useUpdateRecipeBookMutation() {
  const queryClient = useQueryClient();
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: (data: IUpdateRecipeBookArgs & { recipeBookId: string }) =>
      recipeStore.updateRecipeBook(AssertString(userKey), data.recipeBookId, {
        name: data.name,
        versionTag: data.versionTag,
        shortDescription: data.shortDescription,
      }),
    onSuccess: (newData) =>
      updateRecipeBookInCache(queryClient, AssertString(userKey), newData),
  });
}
