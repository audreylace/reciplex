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
