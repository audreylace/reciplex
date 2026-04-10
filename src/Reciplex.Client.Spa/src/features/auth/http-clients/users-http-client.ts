import { HttpClient, HttpError } from "../../core/utils/http-client";

/**
 * Client for the backend http server's endpoints.
 * All methods throw `HttpError` on non-success codes.
 */
export class UsersHttpClient {
  private _client: HttpClient;

  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string) {
    this._client = new HttpClient(prefix + "/v1/users");
  }

  /**
   * the set of accounts the current
   * credentials grant access to
   */
  public async getAccounts(args?: {
    noCache?: boolean;
  }): Promise<IHttpUserJson[]> {
    const response = await this._client.httpGet("", undefined, {
      noCache: args?.noCache,
    });
    return validateOrThrow(
      await response.json(),
      isIHttpUserJsonArray,
      "IHttpUserJson[]",
    );
  }

  /**
   * gets a single user account
   * @param userId the id of the user
   * @returns information about the account or undefined if the account does not exist
   */
  public async getAccount(userId: string): Promise<IHttpUserJson | undefined> {
    let response: Response;
    try {
      response = await this._client.httpGet(`${encodeURIComponent(userId)}`);
    } catch (error) {
      if (error instanceof HttpError && error.status === 404) {
        return undefined;
      }
      throw error;
    }
    return validateOrThrow(
      await response.json(),
      isIHttpUserJson,
      "IHttpUserJson",
    );
  }

  /**
   * Updates an account the user owns
   * @param userId the id of the account to update
   * @param data the new account data
   * @param concurrencyTag the version tag
   * @returns new version of the resource
   */
  public async updateAccount(
    userId: string,
    data: IHttpUserBodyRequest,
    concurrencyTag: string,
  ): Promise<IHttpUserJson> {
    const response = await this._client.httpPut(
      `${encodeURIComponent(userId)}`,
      concurrencyTag,
      data,
    );

    return validateOrThrow(
      await response.json(),
      isIHttpUserJson,
      "IHttpUserJson",
    );
  }

  /**
   * Creates an account the user owns
   * @param data the new account data
   * @returns created resource
   */
  public async createAccount(
    data: IHttpUserBodyRequest,
  ): Promise<IHttpUserJson> {
    const response = await this._client.httpPost("", data);

    return validateOrThrow(
      await response.json(),
      isIHttpUserJson,
      "IHttpUserJson",
    );
  }

  /**
   * Deletes an account the user owns
   * @param userId the id of the account to delete
   * @param concurrencyTag the version tag
   * @returns new version of the resource
   */
  public async deleteAccount(
    userId: string,
    concurrencyTag: string,
  ): Promise<void> {
    await this._client.httpDelete(
      `${encodeURIComponent(userId)}`,
      concurrencyTag,
    );
  }
}

/**
 * Shape of the User JSON
 */
export interface IHttpUserJson {
  /**
   * display name of the user
   */
  displayName: string;
  /**
   * unique key of the user
   */
  userKey: string;
  /**
   * records version
   */
  concurrencyTag: string;
}

/**
 * Shape of the User body JSON request for updating records
 */
export interface IHttpUserBodyRequest {
  /**
   * display name of the user
   */
  displayName: string;
}

/**
 * A Type Guard to verify if an object adheres to the IHttpUserJson shape.
 * Uses 'unknown' to satisfy ESLint and ensure type safety.
 */
function isIHttpUserJson(data: unknown): data is IHttpUserJson {
  // 1. First, check if it's even an object and not null.
  // This is a narrowing step that moves us away from 'unknown'.
  if (typeof data !== "object" || data === null) {
    return false;
  }

  // 2. Now that we know it's an object, cast it to a generic Record.
  // We use 'Record<string, unknown>' because we still don't trust the content,
  // but we need a way to look up keys like 'userKey'.
  const record = data as Record<string, unknown>;

  // 3. Final validation of the specific required properties.
  return (
    typeof record.userKey === "string" &&
    typeof record.displayName === "string" &&
    typeof record.concurrencyTag === "string"
  );
}

/**
 * A Type Guard to verify if an array contains only valid User objects.
 */
function isIHttpUserJsonArray(data: unknown): data is IHttpUserJson[] {
  // We use the first guard to validate every element in the array.
  return Array.isArray(data) && data.every(isIHttpUserJson);
}

/**
 * Exception thrown when the API response does not match the expected TypeScript interface.
 */
export class BadHttpUserJson extends Error {
  private _invalidData: unknown;

  /**
   * @param message A description of what was being validated (e.g., "User List")
   * @param invalidData The actual payload that failed validation
   */
  constructor(message: string, invalidData: unknown) {
    super(`[Schema Violation] ${message} : bad data : ${invalidData}`);
    this._invalidData = invalidData;
  }

  /**
   * The payload that caused the validation to fail.
   * Useful for debugging in the console or logging to a service like Sentry.
   */
  public get invalidData(): unknown {
    return this._invalidData;
  }
}

/**
 * A utility that executes a Type Guard.
 * If the guard passes, it returns the typed data.
 * If it fails, it throws a DataValidationError containing the bad payload.
 *
 * @template T The expected type
 * @param data The unknown data to check
 * @param predicate The Type Guard function (e.g., isIHttpUserJson)
 * @param context A string describing what was being validated for error logging
 * @returns The data cast to type T
 */
export function validateOrThrow<T>(
  data: unknown,
  predicate: (val: unknown) => val is T,
  context: string,
): T {
  if (predicate(data)) {
    return data;
  }

  throw new BadHttpUserJson(context, data);
}
