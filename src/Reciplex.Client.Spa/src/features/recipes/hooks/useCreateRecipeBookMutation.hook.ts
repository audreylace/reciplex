import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeBookArgs } from "../services/recipe-types";
import { getRecipeBookByIdCacheKey } from "./useGetRecipeBookById.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";

/**
 * hook for create recipe book mutation
 * @returns mutation for creating a new recipe book
 */
export function useCreateRecipeBookMutation() {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (args: ICreateRecipeBookArgs) => {
      const result = await recipeStore.createRecipeBook(
        AssertString(userKey),
        args,
      );
      queryClient.setQueryData(getRecipeBookByIdCacheKey(result.id), result);
      return result;
    },
  });
}
