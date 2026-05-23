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
  return [recipeQueryKeyRoot(userKey), recipeCacheKeyBranch, recipeKey];
}

export const recipeBookCacheKeyBranch = "recipeBook";

/**
 * Computes the cache key for a recipe book
 * @param userKey the user requesting the book
 * @param bookKey the key of the book
 * @returns computed query cache key
 */
export function recipeBookQueryKey(userKey: string, bookKey: string) {
  return [recipeQueryKeyRoot(userKey), recipeBookCacheKeyBranch, bookKey];
}

/**
 * computes the cache key for the status of a recipe share request
 * @param userKey the user requesting access to the book
 * @param bookKey the key of the book
 * @param shareKey the share key to authenticate the request
 * @returns computed query cache key
 */
export function recipeBookSharedAccessQueryKey(
  userKey: string,
  bookKey: string,
  shareKey?: string,
) {
  return [
    recipeQueryKeyRoot(userKey),
    "recipeBookSharedAccess",
    bookKey,
    shareKey ?? "",
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
  args?: IRecipeBookListArgsKeyNode,
) {
  return [
    recipeQueryKeyRoot(userKey),
    recipeBookListCacheKeyBranch,
    normalizeRecipeBookListArgsKeyNode(args),
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
    recipeQueryKeyRoot(userKey),
    recipeListCacheKeyBranch,
    normalizeRecipeListArgsKeyNode(args),
  ];
}

export interface IRecipeListArgsKeyNode {
  cursorType: "next" | "previous";
  position?: string;
  pageSize?: number;
  bookKey: string;
}

export interface IRecipeBookListArgsKeyNode {
  cursorType: "next" | "previous";
  position?: string;
  pageSize?: number;
}

function normalizeRecipeListArgsKeyNode(
  args: IRecipeListArgsKeyNode | undefined,
): IRecipeListArgsKeyNode {
  const { cursorType, position, pageSize, bookKey } = args ?? {};
  return {
    cursorType: cursorType ?? "next",
    position,
    pageSize,
    bookKey: bookKey ?? "",
  };
}

function normalizeRecipeBookListArgsKeyNode(
  args: IRecipeBookListArgsKeyNode | undefined,
): IRecipeBookListArgsKeyNode {
  const { cursorType, position, pageSize } = args ?? {};
  return {
    cursorType: cursorType ?? "next",
    position,
    pageSize,
  };
}

/** key for the recipe book access list cache */
export const recipeBookAccessCacheKeyBranch = "recipeBookAccessList";

/**
 * Computes the cache key for a recipe book access list
 * @param userKey the user requesting the recipe
 * @param recipeKey the key of the recipe
 */
export function recipeBookAccessQueryKey(userKey: string, bookKey: string) {
  return [recipeQueryKeyRoot(userKey), recipeBookAccessCacheKeyBranch, bookKey];
}
