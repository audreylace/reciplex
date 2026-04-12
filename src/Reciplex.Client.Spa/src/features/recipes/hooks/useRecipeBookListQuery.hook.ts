import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import {
  recipeBookListQueryKey,
  type IRecipeBookListArgsKeyNode,
} from "../utils/recipe-queries/recipe-query-key-factory";
import type { IGetRecipeBooksArgs } from "../services/recipe-types";

export function useRecipeBookListQuery(
  cursorType: "next" | "previous",
  position: string,
  pageSize?: number,
): ReturnType<typeof useInnerHook>;
export function useRecipeBookListQuery(): ReturnType<typeof useInnerHook>;
export function useRecipeBookListQuery(
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
) {
  return useInnerHook(cursorType, position, pageSize);
}

function buildArgs(
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
): IGetRecipeBooksArgs {
  return {
    cursorType: cursorType ?? "next",
    pageSize,
    position,
  };
}

function buildQueryKeyArgs(
  cursorType?: "next" | "previous",
  position?: string,
  pageSize?: number,
): IRecipeBookListArgsKeyNode {
  return {
    cursorType: cursorType ?? "next",
    pageSize,
    position,
  };
}

function useInnerHook(
  source?: "next" | "previous",
  index?: string,
  pageSize?: number,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const args = buildArgs(source, index, pageSize);
  return useQuery({
    queryKey: recipeBookListQueryKey(
      userKey ?? "",
      buildQueryKeyArgs(source, index, pageSize),
    ),
    enabled: !!userKey,
    queryFn: () => recipeStore.getRecipeBooks(AssertString(userKey), args),
  });
}
