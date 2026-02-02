import {
  ConcurrencyConflict,
  OperationForbidden,
  type ICreateRecipeArgs,
  type ICreateRecipeBookArgs,
  type IGetRecipeBooksArgs,
  type IGetRecipeBooksResult,
  type IGetSessionInformationResult,
  type IRecipeBookModel,
  type IRecipeBookStore,
  type IRecipeModel,
  type IUpdateRecipeArgs,
  type IUserModel,
} from "../../../services/recipe-store";

/**
 * Stores and retrieves recipes from a remote HTTP server
 */
export class RecipeHttpBookStore implements IRecipeBookStore {
  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string) {
    this._prefix = prefix;
  }

  /**
   * First part of the path to the HTTP server
   */
  private _prefix: string;

  /**
   * Cached data
   */
  private _cache: RecipeDataCache = new RecipeDataCache();

  /**
   * @inheritdoc
   */
  async getUserById(userId: string): Promise<IUserModel | null> {
    // TODO - pull data from the remote
    return this._cache.getUser(userId) ?? null;
  }

  /**
   * @inheritdoc
   */
  async getSessionInformation(): Promise<IGetSessionInformationResult> {
    // TODO - pull data from the remote
    return {
      isAuthenticated: true,
      userData: {
        id: "yCgUFTNk",
        displayName: "getSessionInformation -- yCgUFTNk",
      },
    };
  }

  /**
   * @inheritdoc
   */
  async getRecipeBook(
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookModel | null> {
    // Pull from the cache first
    if ((args?.noCache ?? false) === false) {
      const cachedBookData = this._cache.getBook(bookId);
      if (cachedBookData) {
        return cachedBookData;
      }
    }
    var response = await this.httpGet(
      `v1/recipe-books/${encodeURIComponent(bookId)}`,
      undefined,
      { noCache: args?.noCache },
    );

    if (!response.ok) {
      if (response.status === 404) {
        return null;
      }

      if (
        response.status === 400 &&
        (response.headers
          .get("Content-Type")
          ?.indexOf("application/problem+json") ?? -1) != -1
      ) {
        const problemJson: {
          errors?: Record<string, string[]>;
        } = await response.json();
        if (problemJson.errors && problemJson.errors["bookKey"]) {
          // Resolve to not found result when bookId is invalid
          return null;
        }
      }

      throw new Error(
        `unexpected result : ${response.status} : ${await response.text()}`,
      );
    }
    return await this.decodeSingleRecipeBookResult(response);
  }

  /**
   * @inheritdoc
   */
  async getRecipeBooks(
    args?: IGetRecipeBooksArgs,
  ): Promise<IGetRecipeBooksResult | null> {
    let requestPath = "v1/recipe-books";

    const queryParams = [];
    if (args && args.cursor) {
      const cursor = args.cursor;
      if (cursor.position) {
        queryParams.push(`index=${encodeURIComponent(cursor.position)}`);
      }
      queryParams.push(
        `going=${cursor.type === "next" ? "forward" : "backward"}`,
      );
    }

    if (args && (args?.limit ?? 0) > 0) {
      queryParams.push(`page-size=${args.limit}`);
    }

    var response = await this.httpGet(requestPath, queryParams);

    if (!response.ok) {
      await this.throwUnexpectedHttpResult(response);
    }

    const json: IPagedRecipeBookJson | undefined | null = await response.json();

    if (!json) {
      throw new Error(`unexpected result : ${json}`);
    }

    const users: Record<string, IUserModel> = {};
    for (const user of json.users) {
      users[user.userKey] = this.decodeUserJson(user);
    }

    const books: Record<string, IRecipeBookModel> = {};
    for (const book of json.recipeBooks) {
      books[book.bookKey] = this.decodeRecipeBookJson(book);
    }

    return {
      page: json.recipeBooks.map((b) => b.bookKey),
      recipeBooks: books,
      users: users,
      nextCursor: json.nextPage
        ? {
            type: "next",
            position: json.nextPage.index,
          }
        : undefined,
      previousCursor: json.previousPage
        ? {
            type: "previous",
            position: json.previousPage.index,
          }
        : undefined,
    };
  }

  /**
   * @inheritdoc
   */
  async createRecipeBook(
    args: ICreateRecipeBookArgs,
  ): Promise<IRecipeBookModel> {
    const response = await this.httpPost("v1/recipe-books", {
      name: args.name ?? "",
      shortDescription: args.shortDescription ?? "",
    });

    if (!response.ok) {
      await this.throwUnexpectedHttpResult(response);
    }

    return await this.decodeSingleRecipeBookResult(response);
  }

  /**
   * @inheritdoc
   */
  async getRecipeById(
    recipeId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeModel | null> {
    const response = await this.httpGet(
      `v1/recipes/${encodeURIComponent(recipeId)}`,
      undefined,
      { noCache: args?.noCache },
    );

    if (!response.ok) {
      if (response.status === 404) {
        return null;
      }

      if (
        response.status === 400 &&
        (response.headers
          .get("Content-Type")
          ?.indexOf("application/problem+json") ?? -1) != -1
      ) {
        const problemJson: {
          errors?: Record<string, string[]>;
        } = await response.json();
        if (problemJson.errors && problemJson.errors["recipeKey"]) {
          // Resolve to not found result when recipeId is invalid
          return null;
        }
      }
      await this.throwUnexpectedHttpResult(response);
    }
    return await this.decodeSingleRecipeResult(response);
  }

  /**
   * @inheritdoc
   */
  async createRecipe(args: ICreateRecipeArgs): Promise<IRecipeModel> {
    const response = await this.httpPost(
      "v1/recipes",
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
        details: "",
      },
      [`book=${encodeURIComponent(args.bookId)}`],
    );

    if (!response.ok) {
      if (response.status === 403) {
        throw new OperationForbidden();
      }

      await this.throwUnexpectedHttpResult(response);
    }
    return await this.decodeSingleRecipeResult(response);
  }

  /**
   * @inheritdoc
   */
  async deleteRecipe(recipeId: string, versionTag: string): Promise<void> {
    const response = await this.httpDelete(
      `v1/recipes/${encodeURIComponent(recipeId)}`,
      versionTag,
    );

    if (!response.ok) {
      if (response.status === 404) {
        // not found implies it is deleted (mostly)
        return;
      }

      if (response.status === 412) {
        throw new ConcurrencyConflict();
      }

      if (response.status === 403) {
        throw new OperationForbidden();
      }

      await this.throwUnexpectedHttpResult(response);
    }
  }

  /**
   * @inheritdoc
   */
  async deleteRecipeBook(bookId: string, versionTag: string): Promise<void> {
    const response = await this.httpDelete(
      `v1/recipe-books/${encodeURIComponent(bookId)}`,
      versionTag,
    );

    if (!response.ok) {
      // not found implies it is deleted (mostly)
      if (response.status === 404) {
        this._cache.expireBook(bookId);
        return;
      }
      if (response.status === 412) {
        throw new ConcurrencyConflict();
      }

      if (response.status === 403) {
        throw new OperationForbidden();
      }

      await this.throwUnexpectedHttpResult(response);
    }

    this._cache.expireBook(bookId);
  }

  /**
   * @inheritdoc
   */
  async updateRecipe(
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel> {
    const response = await this.httpPut(
      `v1/recipes/${encodeURIComponent(recipeId)}`,
      args.versionTag,
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
        details: args.details,
      },
    );

    if (!response.ok) {
      if (response.status === 412 || response.status === 404) {
        throw new ConcurrencyConflict();
      }

      if (response.status === 403) {
        throw new OperationForbidden();
      }

      await this.throwUnexpectedHttpResult(response);
    }
    return await this.decodeSingleRecipeResult(response);
  }

  /**
   * Gets data from the remote
   * @param path the path
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @param args additional arguments controlling the GET request
   * @returns Promise<Response> object returned from the call to fetch
   */
  private httpGet(
    path: string,
    params?: string[],
    args?: { noCache?: boolean },
  ): Promise<Response> {
    let computedPath = `${this._prefix}/${path}`;
    if (params && params.length > 0) {
      computedPath += `?${params.join("&")}`;
    }

    return fetch(computedPath, {
      credentials: "include",
      cache: args?.noCache ? "reload" : "default",
    });
  }

  /**
   * Posts data to the remote
   * @param path the path
   * @param body the javascript object that will be stringified into the POST body
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @returns Promise<Response> object returned from the call to fetch
   */
  private httpPost<TBody>(
    path: string,
    body: TBody,
    params?: string[],
  ): Promise<Response> {
    let computedPath = `${this._prefix}/${path}`;
    if (params && params.length > 0) {
      computedPath += `?${params.join("&")}`;
    }

    return fetch(computedPath, {
      credentials: "include",
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });
  }

  /**
   * Puts data to the remote
   * @param path the path
   * @param versionTag the version of the resource
   * @param body the javascript object that will be stringified into the PUT body
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @returns Promise<Response> object returned from the call to fetch
   */
  private httpPut<TBody>(
    path: string,
    versionTag: string,
    body: TBody,
    params?: string[],
  ): Promise<Response> {
    let computedPath = `${this._prefix}/${path}`;
    if (params && params.length > 0) {
      computedPath += `?${params.join("&")}`;
    }

    return fetch(computedPath, {
      credentials: "include",
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "If-Match": `\"${versionTag}\"`,
      },
      body: JSON.stringify(body),
    });
  }

  /**
   * Deletes data from the remote
   * @param path the path
   * @param versionTag the version of the resource
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @returns Promise<Response> object returned from the call to fetch
   */
  private httpDelete(
    path: string,
    versionTag: string,
    params?: string[],
  ): Promise<Response> {
    let computedPath = `${this._prefix}/${path}`;
    if (params && params.length > 0) {
      computedPath += `?${params.join("&")}`;
    }

    return fetch(computedPath, {
      credentials: "include",
      method: "DELETE",
      headers: {
        "If-Match": `\"${versionTag}\"`,
      },
    });
  }

  /**
   * Throws an exception for a failed http request
   * @param response the fetch response that failed
   */
  private async throwUnexpectedHttpResult(response: Response): Promise<never> {
    var text = await response.text();
    throw new Error(`unexpected result : ${response.status} : ${text}`);
  }

  /**
   * Decodes a `ISingleRecipeBookJson` into a `IRecipeBookModel` caching it and `owner`.
   * @param response the fetch response
   * @returns the decoded `IRecipeModel`
   */
  async decodeSingleRecipeBookResult(
    response: Response,
  ): Promise<IRecipeBookModel> {
    const json: ISingleRecipeBookJson | undefined | null =
      await response.json();

    if (!json) {
      throw new Error(`unexpected result : ${json}`);
    }

    this.decodeUserJson(json.recipeBookOwner);
    return this.decodeRecipeBookJson(json.recipeBook);
  }

  /**
   * Decodes a `ISingleRecipeJson` into a `IRecipeModel` caching the `recipeBookOwner` and `recipeBook`.
   * @param response the fetch response
   * @returns the decoded `IRecipeModel`
   */
  async decodeSingleRecipeResult(response: Response): Promise<IRecipeModel> {
    const json: ISingleRecipeJson | undefined | null = await response.json();

    if (!json) {
      throw new Error(`unexpected result : ${json}`);
    }

    this.decodeUserJson(json.recipeBookOwner);
    this.decodeRecipeBookJson(json.recipeBook);

    return {
      id: json.recipe.recipeKey,
      name: json.recipe.name,
      shortDescription: json.recipe.shortDescription,
      details: json.recipe.details,
      bookId: json.recipe.bookKey,
      canEditRecipe: json.recipe.mayEdit ?? false,
      canDeleteRecipe: json.recipe.mayEdit ?? false,
      versionTag: json.recipe.concurrencyTag,
    };
  }

  /**
   * decodes the remotes HTTP json into `IRecipeBookModel` and caches it
   * @param recipeBook the source recipe book json
   * @returns the decoded model
   */
  decodeRecipeBookJson(recipeBook: IRecipeBookJson): IRecipeBookModel {
    const book = {
      id: recipeBook.bookKey,
      name: recipeBook.name,
      shortDescription: recipeBook.shortDescription,
      ownerId: recipeBook.owningUserKey,
      canDeleteBook: recipeBook.mayDelete,
      canEditBookInformation: recipeBook.mayEdit,
      canAddRecipesToBook: recipeBook.mayDelete,
      versionTag: recipeBook.concurrencyTag,
    };

    this._cache.cacheBook(book);
    return book;
  }

  /**
   * decodes the remotes HTTP json into `IUserModel` and caches it
   * @param userJson the source user json
   * @returns the decoded model
   */
  decodeUserJson(userJson: IUserJson): IUserModel {
    const user = {
      id: userJson.userKey,
      displayName: userJson.displayName,
    };
    this._cache.cacheUser(user);
    return user;
  }
}

/**
 * JSON returned when requesting a page of recipe books
 */
interface IPagedRecipeBookJson {
  /**
   * the recipe books
   */
  recipeBooks: IRecipeBookJson[];
  /**
   * JSON describing the users linked to by objects in the request
   */
  users: IUserJson[];
  /**
   * populated if there is another page of books
   */
  nextPage?: {
    /**
     * the index used to get the next page
     */
    index: string;
  };
  /**
   * populated if there is a previous page of books
   */
  previousPage?: {
    /**
     * the index used to get the previous page
     */
    index: string;
  };
}

/**
 * JSON returned when requesting a singe recipe book
 */
interface ISingleRecipeBookJson {
  /**
   * the recipe book
   */
  recipeBook: IRecipeBookJson;
  /**
   * the recipe book owner
   */
  recipeBookOwner: IUserJson;
}

/**
 * JSON returned when requesting a single recipe
 */
interface ISingleRecipeJson {
  /**
   * the recipe book
   */
  recipeBook: IRecipeBookJson;
  /**
   * the recipe
   */
  recipe: IRecipeJson;
  /**
   * the recipe book owner
   */
  recipeBookOwner: IUserJson;
}

/**
 * JSON describing a single user
 */
interface IUserJson {
  /**
   * The user's display name
   */
  displayName: string;
  /**
   * Unique ID identifying the user
   */
  userKey: string;
}

/**
 * Recipe book json
 */
interface IRecipeBookJson {
  /**
   * Unique ID of the recipe book
   */
  bookKey: string;

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
  owningUserKey: string;

  /**
   * Tag for optimistic concurrency
   */
  concurrencyTag: string;

  /**
   * Time this book was last modified
   */
  lastModified: string;

  /**
   * Time when this book was created
   */
  created: string;

  /**
   * True when the user can delete the recipe book and all of its data.
   */
  mayDelete?: boolean;

  /**
   * True when the user can edit the recipe book's top level
   * fields and manipulate recipes.
   */
  mayEdit?: boolean;
}

/**
 * Recipe  json
 */
interface IRecipeJson {
  /**
   * Unique ID of the recipe book holding this recipe
   */
  bookKey: string;

  /**
   * Unique ID of the recipe
   */
  recipeKey: string;

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
   * Recipe details
   */
  details: string;

  /**
   * Tag for optimistic concurrency
   */
  concurrencyTag: string;

  /**
   * Time this book was last modified
   */
  lastModified: string;

  /**
   * Time when this book was created
   */
  created: string;

  /**
   * True when the user can edit the recipe book's top level
   * fields and manipulate recipes.
   */
  mayEdit?: boolean;
}

/**
 * Cache entry
 */
type CacheEntry<TType> = {
  /**
   * Cached value
   */
  value: TType;

  /**
   * Future expiration time
   */
  expirationTime: Date;
};

/**
 * Cache type
 */
type Cache<TType> = Record<string, CacheEntry<TType>>;

/**
 * App level cache for recipe data
 */
class RecipeDataCache {
  /**
   * Cached user data
   */
  private _userCache: Cache<IUserModel> = {};

  /**
   * Cached user data
   */
  private _bookCache: Cache<IRecipeBookModel> = {};

  /**
   * handle from setTimeout inside of `scheduleCleanup`
   */
  private _scheduleCleanupTimeoutHandle?: number;

  /**
   * Schedules a cleanup operation if one is not already pending
   */
  private scheduleCleanup(): void {
    if (this._scheduleCleanupTimeoutHandle) {
      return;
    }
    this._scheduleCleanupTimeoutHandle = setTimeout(() => {
      this._scheduleCleanupTimeoutHandle = undefined;
      const now = new Date();
      let scheduleNextCleanup = false;

      let keysToPurge: string[] = [];
      for (const key in this._bookCache) {
        if (this._bookCache[key].expirationTime < now) {
          keysToPurge.push(key);
        } else {
          scheduleNextCleanup = true;
        }
      }
      keysToPurge.forEach((k) => delete this._bookCache[k]);

      keysToPurge = [];
      for (const key in this._userCache) {
        if (this._userCache[key].expirationTime < now) {
          keysToPurge.push(key);
        } else {
          scheduleNextCleanup = true;
        }
      }
      keysToPurge.forEach((k) => delete this._userCache[k]);

      if (scheduleNextCleanup) {
        this.scheduleCleanup();
      }
    }, 61 * 1000);
  }

  /**
   * Invoke to cache user data
   * @param user object to cache
   */
  public cacheUser(user: IUserModel) {
    this.scheduleCleanup();
    this._userCache[user.id] = {
      value: user,
      expirationTime: this.expirationTime(),
    };
  }

  /**
   * Invoke to cache book data
   * @param book object to cache
   */
  public cacheBook(book: IRecipeBookModel) {
    this.scheduleCleanup();
    this._bookCache[book.id] = {
      value: book,
      expirationTime: this.expirationTime(),
    };
  }

  /**
   * Tries to get a user from the cache
   * @param userId the id of the user
   * @returns the `IUserModel` or undefined if it is not cached
   */
  public getUser(userId: string): IUserModel | undefined {
    const user = this._userCache[userId];

    if (!user) {
      return;
    }

    if (user.expirationTime > new Date()) {
      return user.value;
    }

    delete this._userCache[userId];

    return;
  }

  /**
   * Tries to get a book from the cache
   * @param bookId the id of the book
   * @returns the `IRecipeBookModel` or undefined if it is not cached
   */
  public getBook(bookId: string): IRecipeBookModel | undefined {
    const book = this._bookCache[bookId];

    if (!book) {
      return;
    }

    if (book.expirationTime > new Date()) {
      return book.value;
    }

    delete this._bookCache[bookId];

    return;
  }

  /**
   * Expires a book from the cache
   * @param bookId book id to expire
   */
  public expireBook(bookId: string) {
    delete this._bookCache[bookId];
  }

  /**
   * Computes the expiration time of an entry
   * @returns The future expiration time of an entry
   */
  private expirationTime(): Date {
    const now = new Date();

    return new Date(now.getTime() + 60 * 1000);
  }
}
