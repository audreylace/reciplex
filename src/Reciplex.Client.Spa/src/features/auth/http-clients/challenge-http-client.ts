import { HttpClient } from "../../core/utils/http-client";

/**
 * client for interacting with the challenge controller
 */
export class ChallengeHttpClient {
  /**
   * computed challenge path
   */
  private readonly _path: string;

  /**
   * http client for calling the remote server
   */
  private _client: HttpClient;

  /**
   * Class constructor
   * @param prefix API prefix that should not end in a slash
   */
  constructor(prefix: string) {
    this._path = prefix + "/v1/challenge";
    this._client = new HttpClient(this._path);
  }

  /**
   * Starts the challenge sequence
   * changing the location of the page
   * to the challenge endpoint.
   */
  public beginChallenge() {
    window.location.assign(this._path);
  }

  /**
   * Calls the server to determine if an authentication challenge is required.
   * When `true` is returned the client should invoke `beginChallenge`.
   * @returns `true` if a challenge is required.
   */
  public async challengeRequired(): Promise<boolean> {
    const result = (await (await this._client.httpGet("inspect")).json()) as {
      challengeRequired: boolean;
    };

    return result.challengeRequired;
  }
}
