import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { getAccountsQueryKey } from "./useGetAccountsQuery.hook";
import type { IHttpUserJson } from "../http-clients/users-http-client";

export function useCreateAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async ({ displayName }: { displayName: string }, context) => {
      const result = await usersClient.createAccount({ displayName });

      const oldData =
        context.client.getQueryData<IHttpUserJson[]>(getAccountsQueryKey);
      if (oldData) {
        if (!oldData.find((a) => a.userKey === result.userKey)) {
          context.client.setQueryData(getAccountsQueryKey, [
            ...oldData,
            result,
          ]);
        }
      } else {
        context.client.removeQueries({ queryKey: getAccountsQueryKey });
      }
      return result;
    },
  });
}
