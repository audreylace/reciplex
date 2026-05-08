import type { QueryKey } from "@tanstack/react-query";

export const authQueryKeyRoot: { type: "authentication" } = {
  type: "authentication",
};

/** query key for the list of accounts */
export function accountListAuthQueryKey(): QueryKey {
  return [authQueryKeyRoot, "accountList"];
}

/** query key for endpoint to see if a server challenge needs to be issued */
export function challengeAuthQueryKey(): QueryKey {
  return [authQueryKeyRoot, "challenge"];
}
