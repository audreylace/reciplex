import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "../../../hooks/useRecipeStoreContext.hook";
import { BookListNavigationAction } from "../../../route-utils";
import {
  type IGetRecipeBooksArgs,
  CursorTypes,
} from "../../../services/recipe-types";
import { useActiveUserKey } from "../../../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../../../sentinel/stringUtilities";

export function useRecipeBookListQuery(source?: string, index?: string) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
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

      const result = await recipeStore.getRecipeBooks(
        AssertString(userKey),
        args,
      );
      if (!result) {
        throw new Error("Get book API failed");
      }

      return result;
    },
  });
}
