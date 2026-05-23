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
  /**
   * True if the user can share this book.
   */
  mayShare?: boolean;
  /**
   * True if this user can manage who has access to the book.
   */
  mayManageAccess?: boolean;
  /** the key used to authenticate join requests */
  shareKey?: string;
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
 * see `RecipeListEntryJsonResponse.cs`
 */
export interface IRecipeListEntryJsonResponse {
  /**
   * Unique ID of the recipe
   */
  recipeKey: string;
  /**
   * the book this belongs to
   */
  bookKey: string;
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
  pageSize?: number;
  position?: string;
}

/**
 * args for `getRecipesInBook` @see IRecipeBookStore
 */
export interface IGetRecipesInBookArgs {
  cursorType?: CursorTypes;
  /**
   * Limit the number of results
   */
  pageSize?: number;
  position?: string;
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

/** possible states of a recipe book access request */
export const RequestAccessToRecipeBookStatus = {
  /** Not set. Some sort of bug. */
  Unset: 0,
  /** Request approved. */
  Approved: 2,
  /** Request pending. */
  Pending: 1,
  /** Share key is valid and book exists but a request has not been submitted. */
  NoRequestInProgress: 3,
} as const;
type RequestAccessToRecipeBookStatus =
  (typeof RequestAccessToRecipeBookStatus)[keyof typeof RequestAccessToRecipeBookStatus];

/** recipe book access request status */
export interface IRequestAccessToRecipeBookStatus {
  /** the book key */
  bookKey: string;
  /** the book name */
  name: string;
  /** the book short description */
  shortDescription: string;
  /** the request status */
  status: RequestAccessToRecipeBookStatus;
}

/**
 * Model describing a single user's access
 * @see `src/Reciplex.Server.Host/Models/RecipeBookUserPermissionsJsonResponse.cs`
 */
export interface IRecipeBookUserPermissionsJsonResponse {
  /** id of the user this represents */
  userKey: string;
  /** the if of the book */
  bookKey: string;
  /** if the user may view the book */
  mayViewBook: boolean;
  /** if the user may edit book */
  mayEditBook: boolean;
  /** if the entry has been reviewed */
  reviewed: boolean;
  /** the display name */
  userDisplayName: string;
}

/**
 * Model to update a single user
 * @see `src/Reciplex.Server.Host/Models/RecipeBookUserPermissionsJsonRequest.cs`
 */
export interface IRecipeBookUserPermissionsJsonRequest {
  /** if the user may view the book */
  mayViewBook: boolean;
  /** if the user may edit book */
  mayEditBook: boolean;
  /** if the entry has been reviewed */
  reviewed: boolean;
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
  ): Promise<IRecipeBookModel[]>;

  /**
   * gets recipes for a book
   * @param bookId the id of the book
   * @param args optional args to control the query
   */
  getRecipesInBook(
    userId: string,
    bookId: string,
    args?: IGetRecipesInBookArgs,
  ): Promise<IRecipeListEntryJsonResponse[] | null>;

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

  /**
   * updates a recipe book
   * @param userId the id of the user performing the action
   * @param bookId the book id
   * @param args additional arguments
   */
  updateRecipeBook(
    userId: string,
    bookId: string,
    args: IUpdateRecipeBookArgs,
  ): Promise<IRecipeBookModel>;

  /**
   * Mutates a book's share key by calling the remote server
   * @param userId the id of the user performing the action
   * @param bookId the book id
   * @param kind the regeneration kind
   * @param versionTag the version tag for opportunistic concurrency
   */
  updateRecipeBookShareKey(
    userId: string,
    bookId: string,
    kind: "regenerate" | "clear",
    versionTag: string,
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

  /**
   * Gets the status of a request to access a recipe book
   * @param userId the user requesting access
   * @param bookId the id of the book
   * @param shareKey the share key used to validate that the user has an invite
   */
  getRecipeBookShareStatus(
    userId: string,
    bookId: string,
    shareKey?: string,
  ): Promise<IRequestAccessToRecipeBookStatus | null>;

  /**
   * Submits a request to access recipe book
   * @param userId the user requesting access
   * @param bookId the id of the book
   * @param shareKey the share key used to validate that the user has an invite
   */
  postRecipeBookAccessRequest(
    userId: string,
    bookId: string,
    shareKey: string,
  ): Promise<IRequestAccessToRecipeBookStatus | null>;

  /**
   * deletes a book access request
   * @param userId the id of the user
   * @param bookId the id of the book
   */
  deleteRecipeBookAccessRequest(userId: string, bookId: string): Promise<void>;

  /**
   * lists of users with book access
   * @param userId the id of the user running the request
   * @param bookId the id of the book
   */
  getUsersWithBookAccess(
    userId: string,
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookUserPermissionsJsonResponse[] | null>;

  /**
   * patches set of users with access
   * @param userId the id of the user
   * @param bookId the id of the book
   * @param changes the set of changes to make
   */
  patchUsersWithBookAccess(
    userId: string,
    bookId: string,
    changes: [
      string,
      IRecipeBookUserPermissionsJsonRequest | undefined | null,
    ][],
  ): Promise<void>;
}
