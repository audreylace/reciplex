import { HttpClient, HttpError } from "../../core/utils/http-client";
import {
  type ICreateRecipeArgs,
  type ICreateRecipeBookArgs,
  type IGetRecipeBooksArgs,
  type IGetRecipesInBookArgs,
  type IRecipeBookModel,
  type IRecipeBookStore,
  type IRecipeModel,
  type IUpdateRecipeArgs,
  type IUpdateRecipeBookArgs,
} from "./recipe-types";

/**
 * Stores and retrieves recipes from a remote HTTP server
 */
export class RecipeHttpBookStore implements IRecipeBookStore {
  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string) {
    this._bookClient = new HttpClient(prefix + "/v1/recipe-books");
    this._recipeClient = new HttpClient(prefix + "/v1/recipes");
  }

  private _bookClient: HttpClient;
  private _recipeClient: HttpClient;

  private readonly userQueryParam: string = "user";

  private makeQueryParams(userId: string): [string, string][] {
    return [[this.userQueryParam, userId]];
  }

  async getRecipeBook(
    userId: string,
    bookId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeBookModel | null> {
    try {
      const response = await this._bookClient.httpGet(
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
      await this._bookClient.httpGet("", queryParams)
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

  async createRecipeBook(
    userId: string,
    args: ICreateRecipeBookArgs,
  ): Promise<IRecipeBookModel> {
    const response = await this._bookClient.httpPost(
      "",
      {
        name: args.name ?? "",
        shortDescription: args.shortDescription ?? "",
      },
      this.makeQueryParams(userId),
    );

    return this.decodeRecipeBookJson(await response.json());
  }

  async deleteRecipeBook(
    userId: string,
    bookId: string,
    versionTag: string,
  ): Promise<void> {
    await this._bookClient.httpDelete(
      encodeURIComponent(bookId),
      versionTag,
      this.makeQueryParams(userId),
    );
  }

  async updateRecipeBook(
    userId: string,
    bookId: string,
    args: IUpdateRecipeBookArgs,
  ): Promise<IRecipeBookModel> {
    const response = await this._bookClient.httpPut(
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

  async getRecipesInBook(
    userId: string,
    bookId: string,
    args?: IGetRecipesInBookArgs,
  ): Promise<IRecipeModel[] | null> {
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
      response = await this._recipeClient.httpGet("", queryParams);
    } catch (error) {
      if (error instanceof HttpError) {
        if (error.status === 404) {
          return null;
        }
      }

      throw error;
    }

    const recipeJson: IRecipeJson[] | undefined | null = await response.json();

    if (!recipeJson) {
      throw new Error("unexpected recipe json");
    }

    const recipes = recipeJson.map((r) => this.decodeRecipeJson(r));

    if (doReverse) {
      recipes.reverse();
    }

    return recipes;
  }

  async getRecipeById(
    userId: string,
    recipeId: string,
    args?: { noCache?: boolean },
  ): Promise<IRecipeModel | null> {
    try {
      const response = await this._recipeClient.httpGet(
        encodeURIComponent(recipeId),
        this.makeQueryParams(userId),
        { noCache: args?.noCache },
      );
      return this.decodeRecipeJson(await response.json());
    } catch (error) {
      if (
        error instanceof HttpError &&
        (error.status === 404 || error.status === 400)
      ) {
        return null;
      }
      throw error;
    }
  }

  async createRecipe(
    userId: string,
    args: ICreateRecipeArgs,
  ): Promise<IRecipeModel> {
    const response = await this._recipeClient.httpPost(
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

  async deleteRecipe(
    userId: string,
    recipeId: string,
    versionTag: string,
  ): Promise<void> {
    await this._recipeClient.httpDelete(
      encodeURIComponent(recipeId),
      versionTag,
      this.makeQueryParams(userId),
    );
  }

  async updateRecipe(
    userId: string,
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel> {
    const response = await this._recipeClient.httpPut(
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

  decodeRecipeJson(json: IRecipeJson): IRecipeModel {
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

  decodeRecipeBookJson(recipeBook: IRecipeBookJson): IRecipeBookModel {
    const book = {
      id: recipeBook.bookKey,
      name: recipeBook.name,
      shortDescription: recipeBook.shortDescription,
      ownerId: recipeBook.owningUserKey,
      mayDelete: recipeBook.mayDelete,
      mayEdit: recipeBook.mayEdit,
      versionTag: recipeBook.concurrencyTag,
    };

    return book;
  }
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
