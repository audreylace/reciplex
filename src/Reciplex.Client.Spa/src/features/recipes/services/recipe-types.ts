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
  mayDelete?: boolean;
  /**
   * True when the user can edit the recipe book's top level
   * fields and add recipes.
   */
  mayEdit?: boolean;
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
   * True when the user may edit and delete the recipe
   */
  mayEdit: boolean;
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
  nextCursor?: string;
  /** cursor to fetch the previous page */
  previousCursor?: string;
  /**
   * recipe books indexed by key
   */
  books: IRecipeBookModel[];
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
  cursorType?: CursorTypes;
  /**
   * Limit the number of results
   */
  limit?: number;
  position?: string;
}

/**
 * args for `getRecipesInBook` @see IRecipeBookStore
 */
export interface IGetRecipesInBookArgs {
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

/** Args for updating a recipe book */
export interface IUpdateRecipeBookArgs {
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
   * version of the model for optimistic concurrency
   */
  versionTag: string;
}

/**
 * Store for recipe books and recipe data
 */
export interface IRecipeBookStore {
  /**
   * Gets recipe book by id
   * @param bookId the recipe book id
   * @param args optional args for the request
   * @returns a promise that resolves to the book or null if the book does not exist or the user does not have access
   */
  getRecipeBook(
    bookId: string,
    userId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookModel | null>;

  getRecipeBooks(
    userId: string,
    args?: IGetRecipeBooksArgs,
  ): Promise<IGetRecipeBooksResult>;

  /**
   * gets recipes for a book
   * @param bookId the id of the book
   * @param args optional args to control the query
   */
  getRecipesInBook(
    userId: string,
    bookId: string,
    args?: IGetRecipesInBookArgs,
  ): Promise<IGetRecipesInBookResult | null>;

  /**
   * Creates a recipe book
   * @param args args for the create command
   * @returns string with the ID of the new book
   */
  createRecipeBook(
    userId: string,
    args: ICreateRecipeBookArgs,
  ): Promise<IRecipeBookModel>;

  /**
   * get a recipe by id
   * @param recipeId the recipe id
   * @param args optional args for the request
   * @returns promise that resolves to a `IGetRecipeByIdResult`
   * or null if the recipe does not exist (or user does not have access).
   */
  getRecipeById(
    userId: string,
    recipeId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeModel | null>;

  /**
   * Creates a recipe
   * @param args recipe creation args
   */
  createRecipe(userId: string, args: ICreateRecipeArgs): Promise<IRecipeModel>;

  /**
   * Deletes a recipe
   * @param recipeId the recipe id
   * @param versionTag the version tag for optimistic concurrency
   */
  deleteRecipe(
    userId: string,
    recipeId: string,
    versionTag: string,
  ): Promise<void>;

  /**
   * Deletes a recipe book
   * @param bookId the recipe book id
   * @param versionTag the version tag for optimistic concurrency
   */
  deleteRecipeBook(
    userId: string,
    bookId: string,
    versionTag: string,
  ): Promise<void>;

  updateRecipeBook(
    userId: string,
    bookId: string,
    args: IUpdateRecipeBookArgs,
  ): Promise<IRecipeBookModel>;

  /**
   * Updates a recipe
   * @param recipeId id of recipe to load
   * @param args arguments controlling the update
   */
  updateRecipe(
    userId: string,
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel>;
}

/**
 * A single page holding a list of recipes for a book.
 * Use next and previous to determine if there is more data.
 * @todo return user data provided by the server
 */
export interface IGetRecipesInBookResult {
  /** list of recipes */
  recipes: IRecipeModel[];
  /** cursor to fetch the next page */
  nextCursor?: string;
  /** cursor to fetch the previous page */
  previousCursor?: string;
}
