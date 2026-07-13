import { HttpClient, type IHttpClientArgs } from "../../core/utils/http-client";

/** client for triggering a sign out */
export class SignOutHttpClient {
  /** the http client */
  private _client: HttpClient;
  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string, args?: IHttpClientArgs) {
    this._client = new HttpClient(prefix + "/v1/sign-out", args);
  }

  /**
   * triggers a sign out from the remote
   */
  public async signOutAsync(): Promise<void> {
    await this._client.httpPost("", undefined, undefined, {});
  }
}
