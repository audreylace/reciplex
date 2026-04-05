import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import type { IHttpUserJson } from "../http-clients/users-http-client";
import { getAccountsQueryKey } from "./useGetAccountsQuery.hook";

export function useUpdateAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async (
      {
        displayName,
        concurrencyToken,
        userKey,
      }: {
        userKey: string;
        displayName: string;
        concurrencyToken: string;
      },
      context,
    ) => {
      const result = await usersClient.updateAccount(
        userKey,
        { displayName },
        concurrencyToken,
      );

      const oldData =
        context.client.getQueryData<IHttpUserJson[]>(getAccountsQueryKey);
      if (oldData) {
        const accountIndex = oldData.findIndex(
          (a) => a.userKey === result.userKey,
        );
        if (accountIndex !== -1) {
          const newData = oldData.slice(0);
          newData.splice(accountIndex, 1, result);
          context.client.setQueryData(getAccountsQueryKey, newData);
        }
      }
      return result;
    },
  });
}
