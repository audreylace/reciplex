import { useQuery } from "@tanstack/react-query";
import { useContext } from "preact/hooks";
import { type IPageRequestCursor } from "../services/recipe-types";
import { RecipeStore } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";

export function useRecipeForBookQuery(
  bookId: string | undefined | null,
  cursor?: IPageRequestCursor | null,
  pageSize?: number,
) {
  const recipeStore = useContext(RecipeStore);
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

      return await recipeStore.getRecipesInBook(AssertString(bookId), {
        cursor: cursor ?? undefined,
        limit: pageSize,
      });
    },
  });
}
