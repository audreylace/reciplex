/**
 * Additional arguments for the request
 */
export interface IHttpActionArgs {
  /**
   * when true the browser cache is disabled. If not
   * set, then the default state for `fetch` is used.
   */
  noCache?: boolean;
}

/**
 * Client for interacting with a HTTP server.
 */
export class HttpClient {
  /**
   * Constructs a new instance of `HttpClient`
   * @param prefix the base path of all requests
   */
  constructor(prefix: string) {
    this._basePath = prefix;
  }

  /**
   * the base path of all requests
   */
  private _basePath: string;

  /**
   * Gets data from the remote
   * @param path the path
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @param args additional arguments controlling the GET request
   * @returns Promise<Response> object returned from the call to fetch
   */
  public async httpGet(
    path: string,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const computedPath = `${this._basePath}/${path}${this.computeQueryParams(params)}`;

    const response = await fetch(computedPath, {
      credentials: "include",
      cache: args?.noCache ? "reload" : "default",
    });

    return this.throwIfNotSuccessOtherwiseReturn(response);
  }

  /**
   * Posts data to the remote
   * @param path the path
   * @param body the javascript object that will be stringified into the POST body
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @param args additional arguments controlling the GET request
   * @returns Promise<Response> object returned from the call to fetch
   */
  public async httpPost<TBody>(
    path: string,
    body: TBody,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const computedPath = `${this._basePath}/${path}${this.computeQueryParams(params)}`;

    const response = await fetch(computedPath, {
      credentials: "include",
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
      cache: args?.noCache ? "reload" : "default",
    });

    return this.throwIfNotSuccessOtherwiseReturn(response);
  }

  /**
   * Puts data to the remote
   * @param path the path
   * @param versionTag the version of the resource
   * @param body the javascript object that will be stringified into the PUT body
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @param args additional arguments controlling the GET request
   * @returns Promise<Response> object returned from the call to fetch
   */
  public async httpPut<TBody>(
    path: string,
    versionTag: string,
    body: TBody,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const computedPath = `${this._basePath}/${path}${this.computeQueryParams(params)}`;

    const response = await fetch(computedPath, {
      credentials: "include",
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "If-Match": `"${versionTag}"`,
      },
      body: JSON.stringify(body),
      cache: args?.noCache ? "reload" : "default",
    });

    return this.throwIfNotSuccessOtherwiseReturn(response);
  }

  /**
   * Deletes data from the remote
   * @param path the path
   * @param versionTag the version of the resource
   * @param params Additional URL parameters in the form `arg1=value`. Method will combine them and append to end of `path`.
   * @param args additional arguments controlling the GET request
   * @returns Promise<Response> object returned from the call to fetch
   */
  public async httpDelete(
    path: string,
    versionTag: string,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const computedPath = `${this._basePath}/${path}${this.computeQueryParams(params)}`;

    const response = await fetch(computedPath, {
      credentials: "include",
      method: "DELETE",
      headers: {
        "If-Match": `"${versionTag}"`,
      },
      cache: args?.noCache ? "reload" : "default",
    });

    return this.throwIfNotSuccessOtherwiseReturn(response);
  }

  private computeQueryParams(params?: [string, string][]) {
    if (!params || params.length < 1) {
      return "";
    }

    return (
      "?" +
      params
        .map((param: [string, string]) => {
          return `${encodeURIComponent(param[0])}=${encodeURIComponent(param[1])}`;
        })
        .join("&")
    );
  }

  private throwIfNotSuccessOtherwiseReturn(response: Response): Response {
    if (response.status >= 400) {
      throw new HttpError(response);
    }

    return response;
  }
}

/**
 * Base exception for http errors
 */
export class HttpError extends Error {
  /**
   * default constructor
   * @param response the response sourcing the error
   */
  constructor(response: Response) {
    super(
      `remote returned non success response : ${response.status} - ${response.statusText}`,
    );
    this._response = response;
  }

  /**
   * response originating the error
   */
  private _response: Response;

  /**
   * the response sourcing this error
   */
  public get response(): Response {
    return this._response;
  }

  /**
   * status code of the error
   */
  public get status(): number {
    return this._response.status;
  }
}
