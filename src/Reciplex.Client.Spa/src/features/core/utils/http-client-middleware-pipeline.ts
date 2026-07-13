/**
 * Pipeline for http client
 */
export class HttpClientMiddlewarePipeline {
  /** handlers run before a fetch */
  private _beforeFetchHandlers: BeforeFetchHandlerType[] = [];

  /**
   * Invoke before a fetch. Middleware handlers will optionally replace `path` and `args`.
   * @param path the fetch path
   * @param args fetch args
   * @returns final fetch path and args to use
   */
  public async onBeforeFetch(
    path: string,
    args: RequestInit,
  ): Promise<{ path: string; args: RequestInit }> {
    if (this._beforeFetchHandlers.length <= 0) {
      return { path, args };
    }

    let state = { path, args };
    for (let handler of this._beforeFetchHandlers) {
      state = await handler(state.path, state.args);
    }

    return state;
  }

  /**
   * adds a handler to the before fetch pipeline
   * @param handler the handler to add
   */
  public addBeforeFetchHandler(handler: BeforeFetchHandlerType) {
    this._beforeFetchHandlers.push(handler);
  }
}

/**
 * handler ran before a fetch operation is invoked.
 * Returns the path and args the fetch operation should use.
 */
export type BeforeFetchHandlerType = (
  path: string,
  args: RequestInit,
) => Promise<{ path: string; args: RequestInit }>;
