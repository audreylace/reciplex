import type { QueryClient } from "@tanstack/react-query";
import type { IHttpUserJson } from "../http-clients/users-http-client";
import { accountListAuthQueryKey } from "./auth-query-key-factory";

export function maybeUpdateUserListCache(
  client: QueryClient,
  mutator: (
    users: readonly IHttpUserJson[],
  ) => IHttpUserJson[] | readonly IHttpUserJson[] | null,
) {
  const cacheKey = accountListAuthQueryKey();
  const oldData = client.getQueryData<IHttpUserJson[]>(cacheKey);

  if (!oldData) {
    return;
  }

  const result = mutator(oldData);
  if (result === oldData) {
    return;
  }

  if (result === null) {
    client.resetQueries({ queryKey: cacheKey });
    return;
  }

  client.setQueryData(cacheKey, result);
}

export function doUserCacheUpdate(
  queryClient: QueryClient,
  data: IHttpUserJson,
) {
  maybeUpdateUserListCache(queryClient, (prev) => {
    const exists = prev.some((a) => a.userKey === data.userKey);

    if (exists) {
      // Declarative Update: Map through and replace only the matching user
      return prev.map((user) => (user.userKey === data.userKey ? data : user));
    }
    return [...prev, data];
  });
}
