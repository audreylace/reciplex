import { useQuery } from "@tanstack/react-query";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { recipeBookListQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";
import type { IGetRecipeBooksArgs } from "../services/recipe-types";

export function useRecipeBookListQuery(
  source: "next" | "previous",
  index: string,
  pageSize?: number,
): ReturnType<typeof useInnerHook>;
export function useRecipeBookListQuery(): ReturnType<typeof useInnerHook>;
export function useRecipeBookListQuery(
  source?: "next" | "previous",
  index?: string,
  pageSize?: number,
) {
  return useInnerHook(source, index, pageSize);
}

function buildArgs(
  source?: "next" | "previous",
  index?: string,
  pageSize?: number,
): IGetRecipeBooksArgs {
  return {
    cursorType: source ?? "next",
    limit: pageSize,
    position: index,
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
    queryKey: recipeBookListQueryKey(userKey ?? "", args),
    enabled: !!userKey,
    queryFn: () => recipeStore.getRecipeBooks(AssertString(userKey), args),
  });
}
