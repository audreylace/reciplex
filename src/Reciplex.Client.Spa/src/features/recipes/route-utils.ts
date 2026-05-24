/**
 * make a path to the recipe list page for a book
 * @param bookId the book id
 * @returns the path
 */
export function makeViewRecipeBookPath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}`;
}

/**
 * computes a path to the book list page
 * @param args arguments for making the path
 * @returns the path
 */
export function makeBookListPath(args?: {
  cursor: {
    index?: string;
    order: "next" | "previous";
  };
}): string {
  const params = new URLSearchParams();

  if (args?.cursor) {
    params.append("source", args.cursor.order);
    if (args.cursor.index) {
      params.append("index", args.cursor.index);
    }
  }

  const queryString = params.toString();
  return queryString ? `/books?${queryString}` : `/books`;
}

/**
 * computes the path to the create recipe book page
 * @returns the path
 */
export function makeCreateRecipeBookPath(): string {
  return `/books/-/create`;
}

/**
 * computes the path to the create recipe page
 * @param bookId the id of the book the recipe will be added
 * @returns the path
 */
export function makeCreateRecipePath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/-/create`;
}

/**
 * make a path to the recipe edit page
 * @param recipeId the recipe id
 * @returns the path
 */
export function makeEditRecipePath(bookId: string, recipeId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/${encodeURIComponent(recipeId)}/edit`;
}

/**
 * make a path to the recipe view page
 * @param recipeId the recipe id
 * @returns the path
 */
export function makeViewRecipePath(bookId: string, recipeId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/${encodeURIComponent(recipeId)}`;
}

/**
 * path to the root settings page
 * @param bookId the book id
 * @returns the path
 */
export function makeBookSettingsPath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}/settings`;
}

/**
 * make a path to the delete recipe page
 * @param bookId the book id
 * @param recipeId the recipe id
 * @returns the path
 */
export function makeDeleteRecipePath(bookId: string, recipeId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/${encodeURIComponent(recipeId)}/delete`;
}

/**
 * path to the page for managing the share link settings
 * @param bookId the book
 * @returns the path
 */
export function makeShareSettingsPath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}/settings/invitation`;
}

/**
 * Makes the path to the settings page for managing book title, short description and other details
 * @param bookId the book
 * @returns the path
 */
export function makeBookDetailsSettingsPath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}/settings/details`;
}

/**
 * makes the path to the delete page
 * @param bookId the book id
 * @returns the path
 */
export function makeBookDeletePath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}/settings/delete`;
}

/**
 * path to share page
 * @param bookId the book id
 * @param shareKey the share key
 * @returns the computed path
 */
export function makeSharePath(bookId: string, shareKey: string): string {
  return `/books/${encodeURIComponent(bookId)}/invitation/${encodeURIComponent(shareKey)}`;
}

/**
 * path to the page for managing who has access to the book
 * @param bookId the id of the book
 * @returns the computed path
 */
export function makeManageAccessPath(bookId: string) {
  return `/books/${encodeURIComponent(bookId)}/settings/manage-access`;
}

/**
 * Makes the absolute share path. Suitable for end users to send to others when sharing a book.
 * @param bookId the book
 * @param shareKey the share key
 * @returns the path
 */
export function makeAbsoluteSharePath(
  bookId: string,
  shareKey: string,
): string {
  const sharePath = makeSharePath(bookId, shareKey);
  return new URL(sharePath, window.location.href).href;
}
