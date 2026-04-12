import type { IGetRecipeBooksArgs } from "../../services/recipe-types";

/**
 * The root key holding all recipe and recipe book data for
 * a particular user.
 * @param userKey the user who owns this data
 */
export function recipeQueryKeyRoot(userKey: string) {
  return {
    type: "recipes",
    userKey: userKey,
  };
}

export const recipeCacheKeyBranch = "recipe";

/**
 * Computes the cache key for a recipe
 * @param userKey the user requesting the recipe
 * @param recipeKey the key of the recipe
 */
export function recipeQueryKey(userKey: string, recipeKey: string) {
  return [
    {
      type: "recipes",
      userKey: userKey,
    },
    "recipe",
    recipeKey,
  ];
}

/**
 * Computes the cache key for a recipe book
 * @param userKey the user requesting the book
 * @param bookKey the key of the book
 * @returns computed query cache key
 */
export function recipeBookQueryKey(userKey: string, bookKey: string) {
  return [
    {
      type: "recipes",
      userKey: userKey,
    },
    "recipeBook",
    bookKey,
  ];
}

export const recipeBookListCacheKeyBranch = "recipeBookList";

/**
 * Computes the cache key for a page of recipe books
 * @param userKey the key of the user doing the book list lookup
 * @param args inputs controlling the page results
 * @returns computed key
 */
export function recipeBookListQueryKey(
  userKey: string,
  args?: IGetRecipeBooksArgs,
) {
  return [
    {
      type: "recipes",
      userKey: userKey,
    },
    "recipeBookList",
    args ?? {},
  ];
}

export const recipeListCacheKeyBranch = "recipeList";

/**
 * Computes cache key for recipe list request
 * @param userKey the user doing the request
 * @param args the set of arguments for controlling the page request
 * @returns the computed key
 */
export function recipeListQueryKey(
  userKey: string,
  args?: IRecipeListArgsKeyNode,
) {
  return [
    {
      type: "recipes",
      userKey: userKey,
    },
    recipeListCacheKeyBranch,
    args ?? {},
  ];
}

export interface IRecipeListArgsKeyNode {
  cursorType?: "next" | "previous";
  position?: string;
  limit?: number;
  bookKey?: string;
}
