import { useMutation } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeBookArgs } from "../services/recipe-types";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { updateRecipeBookInCache } from "../utils/recipe-queries/recipe-query-helpers";

/**
 * hook for create recipe book mutation
 * @returns mutation for creating a new recipe book
 */
export function useCreateRecipeBookMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (args: ICreateRecipeBookArgs, { client }) => {
      const result = await recipeStore.createRecipeBook(
        AssertString(userKey),
        args,
      );

      updateRecipeBookInCache(client, AssertString(userKey), result, true);
      return result;
    },
  });
}
