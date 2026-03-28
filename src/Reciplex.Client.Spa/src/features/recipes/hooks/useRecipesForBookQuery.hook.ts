import { useQuery } from "@tanstack/react-query";
import { type IPageRequestCursor } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";

export function useRecipeForBookQuery(
  bookId: string | undefined | null,
  cursor?: IPageRequestCursor | null,
  pageSize?: number,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: [
      "feature:recipes",
      "getRecipesInBook",
      { bookId, cursor, pageSize },
    ],
    enabled: !!bookId,
    queryFn: async () => {
      if (!recipeStore) {
        throw new Error("Require recipe store");
      }

      return await recipeStore.getRecipesInBook(
        AssertString(userKey),
        AssertString(bookId),
        {
          cursor: cursor ?? undefined,
          limit: pageSize,
        },
      );
    },
  });
}
