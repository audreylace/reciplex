/**
 * make a path to the recipe list page for a book
 * @param bookId the book id
 * @returns `/recipe-book/{bookId}`
 */
export function makeViewRecipeBookPath(bookId: string): string {
  return `/books/${encodeURIComponent(bookId)}`;
}

/** Navigation actions for makeBookListPath */
export const BookListNavigationAction = {
  /** Cursor for getting the next page of results */
  next: "by_ascending",
  /** Cursor for getting the previous page of results */
  previous: "by_descending",
} as const;
export type BookListNavigationAction =
  (typeof BookListNavigationAction)[keyof typeof BookListNavigationAction];
/**
 * computes a path to the book list page
 * @param args arguments for making the path
 * @returns `/recipe-books/{by_ascending | by_descending}/{cursor.index}?`
 */
export function makeBookListPath(args?: {
  cursor: {
    index?: string;
    order: BookListNavigationAction;
  };
}): string {
  if (!args || !args.cursor) {
    return `/recipe-books`;
  }

  if (
    (args.cursor.order === BookListNavigationAction.previous ||
      args.cursor.order === BookListNavigationAction.next) &&
    args.cursor.index
  ) {
    return `/recipe-books/${args.cursor.order}/${encodeURIComponent(args.cursor.index)}`;
  }

  if (
    args.cursor.order === BookListNavigationAction.previous ||
    args.cursor.order === BookListNavigationAction.next
  ) {
    return `/recipe-books/${args.cursor.order}`;
  }

  throw Error("unable to create book list path");
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
 * @returns `/edit-recipe/{bookId}`
 */
export function makeEditRecipePath(bookId: string, recipeId: string): string {
  return `/books/${encodeURIComponent(bookId)}/recipes/${encodeURIComponent(recipeId)}/edit`;
}

/**
 * make a path to the recipe view page
 * @param recipeId the recipe id
 * @returns `/view-recipe/{bookId}`
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
