import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import type { ICreateRecipeArgs } from "../services/recipe-types";
import { recipeByIdCacheKey } from "./useGetRecipeByIdQuery.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";

export function useCreateRecipeMutation() {
  const userKey = useActiveUserKey();
  const queryClient = useQueryClient();
  const recipeStore = useRecipeStoreContext();
  return useMutation({
    mutationFn: async (data: ICreateRecipeArgs) => {
      const result = await recipeStore.createRecipe(
        AssertString(userKey),
        data,
      );
      queryClient.setQueryData(recipeByIdCacheKey(result.id), result);
      return result;
    },
  });
}
