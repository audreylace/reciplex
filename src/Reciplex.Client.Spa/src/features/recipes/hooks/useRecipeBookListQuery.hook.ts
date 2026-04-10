import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { BookListNavigationAction } from "../route-utils";
import {
  type IGetRecipeBooksArgs,
  CursorTypes,
} from "../services/recipe-types";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { recipeBookListQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

export function useRecipeBookListQuery(
  source?: "next" | "previous",
  index?: string,
  pageSize?: number,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    queryKey: recipeBookListQueryKey(userKey ?? "", {
      cursorType: source ?? "next",
      limit: pageSize,
      position: index,
    }),
    enabled: !!userKey,
    queryFn: async () => {
      if (!recipeStore) {
        throw new Error("Require recipe store");
      }
      const args: IGetRecipeBooksArgs = {
        limit: pageSize,
      };
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
