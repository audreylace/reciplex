/** MAX length of a recipe book name */
export const RecipeBookNameMaxLength = 127;

/** MAX length of a recipe book name */
export const RecipeBookShortDescriptionMaxLength = 255;

/** MAX length of a user display name */
export const UserDisplayNameMaxLength = 127;

/** MAX length of a recipe name */
export const RecipeNameMaxLength = 127;

/** MAX length of a recipe short description */
export const RecipeShortDescriptionMaxLength = 255;

/** MAX length of a recipe details */
export const RecipeDetailsMaxLength = 1024 * 1024; // 1MB

/**
 * Recipe book model
 */
export interface IRecipeBookModel {
  /**
   * Unique ID of the recipe book
   */
  id: string;
  /**
   * version of the model for optimistic concurrency
   */
  versionTag: string;
  /**
   * name of the recipe book
   * @see RecipeBookNameMaxLength Max length of this property
   */
  name: string;
  /**
   * short description of the recipe book
   * @see RecipeBookShortDescriptionMaxLength Max length of this property
   */
  shortDescription: string;
  /**
   * The unique ID of the owner
   */
  ownerId: string;
  /**
   * True when the user can delete the recipe book and all of its data.
   */
  canDeleteBook?: boolean;
  /**
   * True when the user can edit the recipe book's top level
   * fields.
   */
  canEditBookInformation?: boolean;
  /**
   * True when the user may add recipes to the book
   */
  canAddRecipesToBook?: boolean;
}

/**
 * User model
 */
export interface IUserModel {
  /**
   * Unique id of the user
   */
  id: string;
  /**
   * The user's display name. Not unique and user supplied.
   * @see UserDisplayNameMaxLength Max length of this property
   */
  displayName: string;
}

/**
 * Recipe model
 */
export interface IRecipeModel {
  /**
   * Unique ID of the recipe
   */
  id: string;
  /**
   * the book this belongs to
   */
  bookId: string;
  /**
   * name of the recipe
   * @see RecipeNameMaxLength Max length of this property
   */
  name: string;
  /**
   * short description of the recipe
   * @see RecipeShortDescriptionMaxLength Max length of this property
   */
  shortDescription: string;
  /**
   * True when the user may edit the recipe
   */
  canEditRecipe: boolean;
  /**
   * True when the user may delete the recipe
   */
  canDeleteRecipe: boolean;
  /**
   * version of the model for optimistic concurrency
   */
  versionTag: string;
  /**
   * recipe details and other info. Has max length @see RecipeDetailsMaxLength
   */
  details: string;
}

/**
 * results from `getRecipeBooks` @see IRecipeBookStore
 */
export interface IGetRecipeBooksResult {
  /** cursor to fetch the next page */
  nextCursor?: IPageCursor;
  /** cursor to fetch the previous page */
  previousCursor?: IPageCursor;
  /** recipe keys based on the order returned from the server */
  page: string[];
  /**
   * map of user id to user model. Only populated if `fetchUserData` is true
   * and the user is not the current logged in user.
   */
  users?: Record<string, IUserModel>;
  /**
   * recipe books indexed by key
   */
  recipeBooks: Record<string, IRecipeBookModel>;
}

/**
 * A page cursor
 */
export interface IPageCursor {
  /** The cursor's position */
  position: string;
  /** The type of the cursor */
  type: CursorTypes;
}

export interface IPageRequestCursor {
  /** The cursor's position */
  position?: string;
  /** The type of the cursor */
  type: CursorTypes;
}

/** Cursor types */
export const CursorTypes = {
  /** Cursor for getting the next page of results */
  next: "next",
  /** Cursor for getting the previous page of results */
  previous: "previous",
} as const;
/** Cursor types */
export type CursorTypes = (typeof CursorTypes)[keyof typeof CursorTypes];

/**
 * args for `getRecipeBooks` @see IRecipeBookStore
 */
export interface IGetRecipeBooksArgs {
  /**
   * Cursor for getting the next page of data
   */
  cursor?: IPageRequestCursor;
  /**
   * Limit the number of results
   */
  limit?: number;
}
/**
 * result from `getCurrentUser` @see IRecipeBookStore
 */
export interface IGetSessionInformationResult {
  /**
   * If the current session is authenticated
   */
  isAuthenticated: boolean;

  /**
   * Information about the user.
   * Will be null when `isAuthenticated` is false
   * or if the user does not have an account
   * on the server.
   */
  userData?: IUserModel;
}

/**
 * Args for create a recipe book
 */
export interface ICreateRecipeBookArgs {
  /**
   * name of the recipe book
   * @see RecipeBookNameMaxLength Max length of this property
   */
  name: string;
  /**
   * short description of the recipe book
   * @see RecipeBookShortDescriptionMaxLength Max length of this property
   */
  shortDescription?: string;
}

/**
 * Args for create a recipe
 */
export interface ICreateRecipeArgs {
  /**
   * name of the recipe book
   * @see RecipeBookNameMaxLength Max length of this property
   */
  name: string;
  /**
   * short description of the recipe book
   * @see RecipeBookShortDescriptionMaxLength Max length of this property
   */
  shortDescription?: string;
  /**
   * The parent book of the recipe
   */
  bookId: string;
}

export interface IUpdateRecipeArgs {
  /**
   * name of the recipe
   * @see RecipeNameMaxLength Max length of this property
   */
  name: string;
  /**
   * short description of the recipe
   * @see RecipeShortDescriptionMaxLength Max length of this property
   */
  shortDescription: string;
  /**
   * version of the model for optimistic concurrency
   */
  versionTag: string;
  /**
   * recipe details and other info. Has max length @see RecipeDetailsMaxLength
   */
  details: string;
}

/**
 * Store for recipe books and recipe data
 */
export interface IRecipeBookStore {
  /**
   * Gets information for a user by id
   * @param userId the user id
   * @param args optional args for the request
   * @returns a promise that resolves either to null or with user information
   */
  getUserById(
    userId: string,
    args?: { noCache?: boolean },
  ): Promise<IUserModel | null>;

  /**
   * Returns information about the current session.
   * @returns a promise about the current user. Resolves to null if
   * the use is not logged in or does not have an account.
   */
  getSessionInformation(): Promise<IGetSessionInformationResult>;

  /**
   * Gets recipe book by id
   * @param bookId the recipe book id
   * @param args optional args for the request
   * @returns a promise that resolves to the book or null if the book does not exist or the user does not have access
   */
  getRecipeBook(
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookModel | null>;

  getRecipeBooks(
    args?: IGetRecipeBooksArgs,
  ): Promise<IGetRecipeBooksResult | null>;

  /**
   * Creates a recipe book
   * @param args args for the create command
   * @returns string with the ID of the new book
   */
  createRecipeBook(args: ICreateRecipeBookArgs): Promise<IRecipeBookModel>;

  /**
   * get a recipe by id
   * @param recipeId the recipe id
   * @param args optional args for the request
   * @returns promise that resolves to a `IGetRecipeByIdResult`
   * or null if the recipe does not exist (or user does not have access).
   */
  getRecipeById(
    recipeId: string,
    args?: { noCache?: boolean },
  ): Promise<IGetRecipeByIdResult | null>;

  /**
   * Creates a recipe
   * @param args recipe creation args
   */
  createRecipe(args: ICreateRecipeArgs): Promise<IRecipeModel>;

  /**
   * Deletes a recipe
   * @param recipeId the recipe id
   * @param versionTag the version tag for optimistic concurrency
   */
  deleteRecipe(recipeId: string, versionTag: string): Promise<void>;

  /**
   * Deletes a recipe book
   * @param bookId the recipe book id
   * @param versionTag the version tag for optimistic concurrency
   */
  deleteRecipeBook(bookId: string, versionTag: string): Promise<void>;

  /**
   * Updates a recipe
   * @param args recipe update args
   */
  updateRecipe(
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel>;
}

export class ConcurrencyConflict extends Error {
  constructor() {
    super("request failed because of concurrency conflict");
  }
}

export class OperationForbidden extends Error {
  constructor() {
    super("request failed because the operation is forbidden");
  }
}
export interface IGetRecipeByIdResult {
  recipe: IRecipeModel;
  book: IRecipeBookModel;
}
