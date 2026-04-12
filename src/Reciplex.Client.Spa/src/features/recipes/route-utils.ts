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
 * make a path to the delete recipe page
 * @param bookId the book id
 * @param recipeId the recipe id
 * @returns the path
 */
export function makeDeleteRecipePath(bookId: string, recipeId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/${encodeURIComponent(recipeId)}/delete`;
}
