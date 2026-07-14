import { HttpClient, type IHttpClientArgs } from "../../core/utils/http-client";

/** client for getting the xsrf token */
export class XsrfHttpClient {
  /** the http client */
  private _client: HttpClient;
  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string, args?: IHttpClientArgs) {
    this._client = new HttpClient(prefix + "/v1/xsrf", args);
  }

  /**
   * gets the xsrf token from the remote
   */
  public async getXsrfToken(): Promise<string> {
    const response = await this._client.httpPost("", undefined, undefined, {
      headers: {
        "X-Requested-With": "reciplex js",
      },
    });
    const jsonData = (await response.json()) as { xsrfToken: string };
    return jsonData.xsrfToken;
  }
}
