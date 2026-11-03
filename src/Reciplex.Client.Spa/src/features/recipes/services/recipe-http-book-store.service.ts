import { HttpClient, type IHttpClientArgs } from "../../core/utils/http-client";
import { HttpError } from "../../core/utils/http-error";
import {
  type ICreateRecipeArgs,
  type ICreateRecipeBookArgs,
  type IGetRecipeBooksArgs,
  type IGetRecipesInBookArgs,
  type IRecipeBookModel,
  type IRecipeBookStore,
  type IRecipeBookUserPermissionsJsonRequest,
  type IRecipeBookUserPermissionsJsonResponse,
  type IRecipeListEntryJsonResponse,
  type IRecipeModel,
  type IRequestAccessToRecipeBookStatus,
  type ISearchRecipeAdditionalArgs,
  type IUpdateRecipeArgs,
  type IUpdateRecipeBookArgs,
} from "./recipe-types";

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

  /**
   * True if the user can share this book.
   */
  mayShare?: boolean;

  /**
   * True if this user can manage who has access to the book.
   */
  mayManageAccess?: boolean;

  /**
   * The share key used to authenticate access
   */
  shareKey?: string;
}

/**
 * Recipe json
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
 * Stores and retrieves recipes from a remote HTTP server
 */
export class RecipeHttpBookStore implements IRecipeBookStore {
  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   * @param args args for the http client
   */
  constructor(prefix: string, args?: IHttpClientArgs) {
    this._bookHttpClient = new HttpClient(prefix + "/v1/recipe-books", args);
    this._recipeHttpClient = new HttpClient(prefix + "/v1/recipes", args);
    this._recipeSearchHttpClient = new HttpClient(
      prefix + "/v1/search/recipes",
      args,
    );
  }

  /** Http client for fetching books */
  private readonly _bookHttpClient: HttpClient;

  /** Http client for fetching recipes */
  private readonly _recipeHttpClient: HttpClient;

  /**
   * Http client for the remote search API used to search
   * recipes.
   */
  private readonly _recipeSearchHttpClient: HttpClient;

  /** name of the user id query parameter */
  private readonly userQueryParam: string = "user";

  /**
   * Initializes the query parameter array with the user id `userId`
   * @param userId the id of the user making the request
   * @returns the initialized query array
   */
  private makeQueryParams(userId: string): [string, string][] {
    return [[this.userQueryParam, userId]];
  }

  /** @inheritdoc */
  async getRecipeBook(
    userId: string,
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookModel | null> {
    try {
      const response = await this._bookHttpClient.httpGet(
        encodeURIComponent(bookId),
        this.makeQueryParams(userId),
        { noCache: args?.noCache },
      );

      return this.decodeRecipeBookJson(await response.json());
    } catch (error) {
      if (error instanceof HttpError) {
        if (error.status === 404) {
          return null;
        }
      }

      throw error;
    }
  }

  /** @inheritdoc */
  async getRecipeBooks(
    userId: string,
    args?: IGetRecipeBooksArgs,
  ): Promise<IRecipeBookModel[]> {
    const queryParams: [string, string][] = this.makeQueryParams(userId);

    let reverse = false;
    if (args) {
      const { cursorType, pageSize, position } = args;
      let positionQuery;
      if (cursorType === "previous") {
        reverse = true;
        queryParams.push(["result-ordering", "id-decreasing"]);
        positionQuery = "before-id";
      } else if (cursorType === "next" || typeof cursorType === "undefined") {
        queryParams.push(["result-ordering", "id-increasing"]);
        positionQuery = "after-id";
      } else {
        throw new Error("unrecognized cursor type");
      }

      if (position) {
        queryParams.push([positionQuery, position]);
      }

      if ((pageSize ?? 0) > 0) {
        queryParams.push(["page-size", `${pageSize}`]);
      }
    }

    const responseJson: IRecipeBookJson[] | null | undefined = await (
      await this._bookHttpClient.httpGet("", queryParams)
    ).json();

    if (!responseJson) {
      throw Error("bad json");
    }

    const books = responseJson.map((b: IRecipeBookJson) =>
      this.decodeRecipeBookJson(b),
    );

    if (reverse) {
      books.reverse();
    }

    return books;
  }

  /** @inheritdoc */
  async createRecipeBook(
    userId: string,
    args: ICreateRecipeBookArgs,
  ): Promise<IRecipeBookModel> {
    const response = await this._bookHttpClient.httpPost(
      "",
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
      },
      this.makeQueryParams(userId),
    );

    return this.decodeRecipeBookJson(await response.json());
  }

  /** @inheritdoc */
  async deleteRecipeBook(
    userId: string,
    bookId: string,
    versionTag: string,
  ): Promise<void> {
    await this._bookHttpClient.httpDelete(
      encodeURIComponent(bookId),
      versionTag,
      this.makeQueryParams(userId),
    );
  }

  /** @inheritdoc */
  async updateRecipeBook(
    userId: string,
    bookId: string,
    args: IUpdateRecipeBookArgs,
  ): Promise<IRecipeBookModel> {
    const response = await this._bookHttpClient.httpPut(
      encodeURIComponent(bookId),
      args.versionTag,
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
      },
      this.makeQueryParams(userId),
    );

    return this.decodeRecipeBookJson(await response.json());
  }

  /** @inheritdoc */
  async updateRecipeBookShareKey(
    userId: string,
    bookId: string,
    kind: "regenerate" | "clear",
    versionTag: string,
  ): Promise<IRecipeBookModel> {
    const response = await this._bookHttpClient.httpPost(
      `${encodeURIComponent(bookId)}/shared-access/-/share-key`,
      undefined,
      [...this.makeQueryParams(userId), ["kind", kind]],
      { headers: { "If-Match": `"${versionTag}"` } },
    );

    return this.decodeRecipeBookJson(await response.json());
  }

  /** @inheritdoc */
  async getRecipeBookShareStatus(
    userId: string,
    bookId: string,
    shareKey?: string,
  ): Promise<IRequestAccessToRecipeBookStatus | null> {
    try {
      const response = await this._bookHttpClient.httpGet(
        `${encodeURIComponent(bookId)}/shared-access/${encodeURIComponent(userId)}`,
        shareKey ? [["share-key", shareKey]] : undefined,
      );
      return await response.json();
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /** @inheritdoc */
  async postRecipeBookAccessRequest(
    userId: string,
    bookId: string,
    shareKey: string,
  ): Promise<IRequestAccessToRecipeBookStatus | null> {
    try {
      const response = await this._bookHttpClient.httpPost(
        `${encodeURIComponent(bookId)}/shared-access/${encodeURIComponent(userId)}`,
        undefined,
        shareKey ? [["share-key", shareKey]] : undefined,
      );
      return await response.json();
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /** @inheritdoc */
  async deleteRecipeBookAccessRequest(
    userId: string,
    bookId: string,
  ): Promise<void> {
    try {
      await this._bookHttpClient.httpDelete(
        `${encodeURIComponent(bookId)}/shared-access/${encodeURIComponent(userId)}`,
      );
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return;
      }
      throw error;
    }
  }

  /** @inheritdoc */
  async getRecipesInBook(
    userId: string,
    bookId: string,
    args?: IGetRecipesInBookArgs,
  ): Promise<IRecipeListEntryJsonResponse[] | null> {
    const queryParams = this.makeQueryParams(userId);
    queryParams.push(["book", bookId]);

    let doReverse = false;

    if (args) {
      const { position, cursorType, pageSize } = args;
      let positionQuery;
      if (cursorType === "next") {
        queryParams.push(["result-ordering", "id-increasing"]);
        positionQuery = "after-id";
      } else if (cursorType === "previous") {
        queryParams.push(["result-ordering", "id-decreasing"]);
        positionQuery = "before-id";
        doReverse = true;
      }

      if (position && positionQuery) {
        queryParams.push([positionQuery, position]);
      }

      if ((pageSize ?? 0) > 0) {
        queryParams.push(["page-size", `${pageSize}`]);
      }
    }

    let response: Response;
    try {
      response = await this._recipeHttpClient.httpGet("", queryParams);
    } catch (error) {
      if (error instanceof HttpError) {
        if (error.status === 404) {
          return null;
        }
      }

      throw error;
    }

    const recipeJson: IRecipeListEntryJsonResponse[] | undefined | null =
      await response.json();

    if (!recipeJson) {
      throw new Error("unexpected recipe json");
    }

    if (doReverse) {
      recipeJson.reverse();
    }

    return recipeJson;
  }

  /** @inheritdoc */
  async getRecipeById(
    userId: string,
    recipeId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeModel | null> {
    try {
      const response = await this._recipeHttpClient.httpGet(
        encodeURIComponent(recipeId),
        this.makeQueryParams(userId),
        { noCache: args?.noCache },
      );
      return this.decodeRecipeJson(await response.json());
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /** @inheritdoc */
  async createRecipe(
    userId: string,
    args: ICreateRecipeArgs,
  ): Promise<IRecipeModel> {
    const response = await this._recipeHttpClient.httpPost(
      "",
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
        details: "",
      },
      [...this.makeQueryParams(userId), ["book", args.bookId]],
    );

    return this.decodeRecipeJson(await response.json());
  }

  /** @inheritdoc */
  async deleteRecipe(
    userId: string,
    recipeId: string,
    versionTag: string,
  ): Promise<void> {
    await this._recipeHttpClient.httpDelete(
      encodeURIComponent(recipeId),
      versionTag,
      this.makeQueryParams(userId),
    );
  }

  /** @inheritdoc */
  async updateRecipe(
    userId: string,
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel> {
    const response = await this._recipeHttpClient.httpPut(
      encodeURIComponent(recipeId),
      args.versionTag,
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
        details: args.details,
      },
      this.makeQueryParams(userId),
    );

    return this.decodeRecipeJson(await response.json());
  }

  /** @inheritdoc */
  async getUsersWithBookAccess(
    userId: string,
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookUserPermissionsJsonResponse[] | null> {
    try {
      const response = await this._bookHttpClient.httpGet(
        `${encodeURIComponent(bookId)}/shared-access`,
        this.makeQueryParams(userId),
        { noCache: args?.noCache },
      );
      return await response.json();
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /** @inheritdoc */
  async patchUsersWithBookAccess(
    userId: string,
    bookId: string,
    changes: [
      string,
      IRecipeBookUserPermissionsJsonRequest | undefined | null,
    ][],
  ): Promise<void> {
    await this._bookHttpClient.httpPatch(
      `${encodeURIComponent(bookId)}/shared-access`,
      changes.map((v) => {
        return { key: v[0], value: v[1] ?? null };
      }),
      this.makeQueryParams(userId),
    );
  }

  /** @inheritdoc */
  async searchRecipes(
    userId: string,
    searchString: string,
    args?: ISearchRecipeAdditionalArgs,
  ): Promise<IRecipeListEntryJsonResponse[] | null> {
    const queryParams = this.makeQueryParams(userId);

    searchString = searchString.trim();
    if (!searchString) {
      // skip calling server if length is empty;
      // also server will reject our call
      return [];
    }

    queryParams.push(["q", searchString]);

    if (args) {
      if (args.bookId) {
        queryParams.push(["book", args.bookId]);
      }
      if (args.limit && args.limit > 0) {
        queryParams.push(["limit", args.limit.toString()]);
      }
      if (args.offset && args.offset > 0) {
        queryParams.push(["offset", args.offset.toString()]);
      }
    }

    try {
      const response = await this._recipeSearchHttpClient.httpGet(
        "",
        queryParams,
      );
      const recipeJson: IRecipeListEntryJsonResponse[] | undefined | null =
        await response.json();

      if (!recipeJson) {
        throw new Error("unexpected recipe json");
      }

      return recipeJson;
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Decodes the response from the server into the client side model
   * @param json the response from the remote server
   * @returns the decoded model
   */
  private decodeRecipeJson(json: IRecipeJson): IRecipeModel {
    const recipe = {
      id: json.recipeKey,
      name: json.name,
      shortDescription: json.shortDescription,
      details: json.details,
      bookId: json.bookKey,
      mayEdit: json.mayEdit ?? false,
      versionTag: json.concurrencyTag,
    };

    return recipe;
  }

  /**
   * decodes the response from the server into the model
   * expected by the client
   * @param recipeBook the incoming json from the remote server
   * @returns the decoded model used by the client application
   */
  private decodeRecipeBookJson(recipeBook: IRecipeBookJson): IRecipeBookModel {
    const book: IRecipeBookModel = {
      id: recipeBook.bookKey,
      name: recipeBook.name,
      shortDescription: recipeBook.shortDescription,
      ownerId: recipeBook.owningUserKey,
      mayDelete: recipeBook.mayDelete,
      mayEdit: recipeBook.mayEdit,
      versionTag: recipeBook.concurrencyTag,
      mayShare: recipeBook.mayShare,
      mayManageAccess: recipeBook.mayManageAccess,
      shareKey: recipeBook.shareKey,
    };

    return book;
  }
}
