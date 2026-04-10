import { useQuery } from "@tanstack/react-query";
import { type IPageRequestCursor } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { recipeListQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

export function useRecipeForBookQuery(
  bookId: string | undefined | null,
  cursor?: IPageRequestCursor | null,
  pageSize?: number,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: recipeListQueryKey(userKey ?? "", {
      bookKey: bookId ?? "",
      position: cursor?.position,
      cursorType: cursor?.type ?? "next",
      limit: pageSize,
    }),
    enabled: !!bookId && !!userKey,
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
