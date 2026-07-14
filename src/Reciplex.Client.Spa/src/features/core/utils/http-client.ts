import { HttpClientMiddlewarePipeline } from "./http-client-middleware-pipeline";
import { HttpError } from "./http-error";

/**
 * Additional arguments for the request
 */
export interface IHttpActionArgs {
  /**
   * when true the browser cache is disabled. If not
   * set, then the default state for `fetch` is used.
   */
  noCache?: boolean;
  /** headers for the request */
  headers?: Record<string, string>;
}

/** optional args for `IHttpClient` */
export interface IHttpClientArgs {
  /** optional pipeline to intercept and modify actions taken by the client */
  pipeline?: HttpClientMiddlewarePipeline;
}

/**
 * Client for interacting with a HTTP server.
 */
export class HttpClient {
  /**
   * Constructs a new instance of `HttpClient`
   * @param prefix the base path of all requests
   * @param args optional args to customize the client
   */
  constructor(prefix: string, args?: IHttpClientArgs) {
    this._basePath = prefix;
    this._pipeline = args?.pipeline ?? new HttpClientMiddlewarePipeline();
  }

  /**
   * the base path of all requests
   */
  private _basePath: string;

  /** the http pipeline for this client */
  private _pipeline: HttpClientMiddlewarePipeline;

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
    return this.request("GET", path, params, args);
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
    body?: TBody,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const headers = args?.headers ?? {};
    return this.request("POST", path, params, args, headers, body);
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
    const headers = args?.headers ?? {};
    headers["If-Match"] = `"${versionTag}"`;
    return this.request("PUT", path, params, args, headers, body);
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
    versionTag?: string,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const headers = args?.headers ?? {};
    if (versionTag) {
      headers["If-Match"] = `"${versionTag}"`;
    }
    return this.request("DELETE", path, params, args, headers);
  }

  private computeQueryParams(params?: [string, string][]): string {
    if (!params || params.length === 0) {
      return "";
    }

    const searchParams = new URLSearchParams();

    // Iterating and appending is the most type-safe way to
    // satisfy the TypeScript compiler across all environments.
    for (const [key, value] of params) {
      searchParams.append(key, value);
    }

    const queryString = searchParams.toString();
    return queryString ? `?${queryString}` : "";
  }

  /**
   * runs a patch request
   * @param path the path
   * @param body the body
   * @param params query parameters
   * @param args arguments to control the client behavior
   * @returns http response
   */
  public async httpPatch<TBody>(
    path: string,
    body: TBody,
    params?: [string, string][],
    args?: IHttpActionArgs,
  ): Promise<Response> {
    const headers = args?.headers ?? {};
    return this.request("PATCH", path, params, args, headers, body);
  }

  /**
   * The core engine for all HTTP calls.
   * Consolidates path computation, header management, and error handling.
   */
  private async request<T>(
    method: string,
    path: string,
    params?: [string, string][],
    args?: IHttpActionArgs,
    customHeaders?: Record<string, string>,
    body?: T,
  ): Promise<Response> {
    const computedPath = `${this._basePath}/${path}${this.computeQueryParams(params)}`;

    // Start with only the custom headers provided (e.g., If-Match)
    const headers: Record<string, string> = {
      ...customHeaders,
    };

    // ONLY add Content-Type if there is actually a body to describe
    if (body) {
      headers["Content-Type"] = "application/json";
    }

    const pipelineResult = await this._pipeline.onBeforeFetch(computedPath, {
      method,
      credentials: "include",
      headers,
      cache: args?.noCache ? "reload" : "default",
      body: body ? JSON.stringify(body) : undefined,
    });

    const response = await fetch(pipelineResult.path, pipelineResult.args);

    return this.throwIfNotSuccessOtherwiseReturn(response);
  }

  private throwIfNotSuccessOtherwiseReturn(response: Response): Response {
    if (response.status >= 400) {
      throw new HttpError(response);
    }

    return response;
  }
}
