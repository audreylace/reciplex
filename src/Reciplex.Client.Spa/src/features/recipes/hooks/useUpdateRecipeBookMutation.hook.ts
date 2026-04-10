import { useMutation } from "@tanstack/react-query";
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
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (
      data: IUpdateRecipeBookArgs & { recipeBookId: string },
      { client },
    ) => {
      const newData = await recipeStore.updateRecipeBook(
        AssertString(userKey),
        data.recipeBookId,
        {
          name: data.name,
          versionTag: data.versionTag,
          shortDescription: data.shortDescription,
        },
      );
      updateRecipeBookInCache(client, AssertString(userKey), newData);
      return newData;
    },
  });
}
