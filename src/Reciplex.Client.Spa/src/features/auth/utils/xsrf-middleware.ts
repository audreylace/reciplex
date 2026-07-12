import type { BeforeFetchHandlerType } from "../../core/utils/http-client";
import type { XsrfHttpClient } from "../http-clients/xsrf-http-client";

/** middleware for populating xsrf tokens on requests */
export class XsrfMiddleware {
  /**
   * constructs this class
   * @param client instance for getting xsrf tokens from the remote
   */
  constructor(client: XsrfHttpClient) {
    this._xsrfClient = client;
  }
  /** cached xsrf token */
  private _xsrfToken?: string;

  /** client for getting xsrf data from the remote */
  private _xsrfClient: XsrfHttpClient;

  /** requests waiting on xsrf token retrieval */
  private _pendingPromises: {
    resolve: (value: string) => void;
    reject: (value: any) => void;
  }[] = [];

  /** token for the in progress xsrf retrieval */
  private _inProgress: object | null = null;

  /**
   * handler for the http pipeline
   * @returns the delegate to pass to the http pipeline
   */
  public onBeforeFetchHandle(): BeforeFetchHandlerType {
    return async (path: string, args: RequestInit) => {
      if (!args.method || args.method === "GET") {
        return { path, args };
      }

      const token = await this.getToken();

      const headers = new Headers(args.headers);
      headers.set("X-XSRF-TOKEN", token);
      return { path, args: { ...args, headers } };
    };
  }

  /**
   * refreshes the token from the remote
   */
  public refreshToken() {
    this.populateToken();
  }

  /**
   * gets the token from the remote
   * @returns promise that resolves with the token value
   */
  private getToken(): Promise<string> {
    if (this._xsrfToken) {
      return Promise.resolve(this._xsrfToken);
    }

    if (!this._inProgress) {
      this.populateToken();
    }

    return new Promise((resolve, reject) => {
      this._pendingPromises.push({ resolve, reject });
    });
  }

  /**
   * runs an async operation to populate the token
   */
  private async populateToken() {
    const myToken = {};
    this._inProgress = myToken;
    this._xsrfToken = undefined;
    try {
      const token = await this._xsrfClient.getXsrfToken();
      if (this._inProgress === myToken) {
        this._xsrfToken = token;
        this._pendingPromises.forEach((e) => {
          try {
            e.resolve(token);
          } catch (error) {
            console.log(error);
          }
        });
      }
    } catch (error) {
      if (this._inProgress === myToken) {
        this._pendingPromises.forEach((e) => {
          try {
            e.reject(error);
          } catch {
            console.log(error);
          }
        });
      }
    } finally {
      if (this._inProgress === myToken) {
        this._inProgress = null;
        this._pendingPromises = [];
      }
    }
  }
}
