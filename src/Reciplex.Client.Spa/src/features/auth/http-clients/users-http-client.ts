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
    return (await response.json()) as IHttpUserJson[];
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
    return (await response.json()) as IHttpUserJson;
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

    return (await response.json()) as IHttpUserJson;
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

    return (await response.json()) as IHttpUserJson;
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
