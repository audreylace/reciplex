import { useQuery } from "@tanstack/react-query";
import { useContext } from "preact/hooks";
import { RecipeStore } from "../../../hooks/useRecipeStoreContext.hook";
import { BookListNavigationAction } from "../../../route-utils";
import {
  type IGetRecipeBooksArgs,
  CursorTypes,
} from "../../../../../services/recipe-store";

export function useRecipeBookListQuery(source?: string, index?: string) {
  const recipeStore = useContext(RecipeStore);
  return useQuery({
    queryKey: ["recipe-book-list", { source, index }],
    queryFn: async () => {
      if (!recipeStore) {
        throw new Error("Require recipe store");
      }
      const args: IGetRecipeBooksArgs = {};
      if (
        source &&
        (source === BookListNavigationAction.next ||
          source === BookListNavigationAction.previous)
      ) {
        args.cursor = {
          position: index,
          type:
            source === BookListNavigationAction.next
              ? CursorTypes.next
              : CursorTypes.previous,
        };
      }

      const result = await recipeStore.getRecipeBooks(args);
      if (!result) {
        throw new Error("Get book API failed");
      }

      return result;
    },
  });
}
