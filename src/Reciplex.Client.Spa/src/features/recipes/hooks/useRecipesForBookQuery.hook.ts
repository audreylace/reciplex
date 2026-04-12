import { useQuery } from "@tanstack/react-query";
import { type IGetRecipesInBookArgs } from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import {
  recipeListQueryKey,
  type IRecipeListArgsKeyNode,
} from "../utils/recipe-queries/recipe-query-key-factory";

export function useRecipeForBookQuery(
  bookId: string,
): ReturnType<typeof useInnerHook>;
export function useRecipeForBookQuery(
  bookId: string,
  cursorType: "next" | "previous",
  position: string,
  pageSize?: number,
): ReturnType<typeof useInnerHook>;
export function useRecipeForBookQuery(
  bookId: string,
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
) {
  return useInnerHook(bookId, cursorType, position, pageSize);
}

function useInnerHook(
  bookId: string,
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const enabled = !!bookId && !!userKey;
  return useQuery({
    queryKey: recipeListQueryKey(
      userKey ?? "",
      makeQueryKeyArgs(bookId, cursorType, position, pageSize),
    ),
    enabled: enabled,
    queryFn: async () => {
      return await recipeStore.getRecipesInBook(
        AssertString(userKey),
        AssertString(bookId),
        makeApiArgs(cursorType, position, pageSize),
      );
    },
  });
}

function makeApiArgs(
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
): IGetRecipesInBookArgs {
  return {
    cursorType: cursorType ?? "next",
    position,
    pageSize,
  };
}

function makeQueryKeyArgs(
  bookId: string | undefined,
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
): IRecipeListArgsKeyNode {
  return {
    bookKey: bookId ?? "",
    cursorType: cursorType ?? "next",
    position,
    pageSize,
  };
}
