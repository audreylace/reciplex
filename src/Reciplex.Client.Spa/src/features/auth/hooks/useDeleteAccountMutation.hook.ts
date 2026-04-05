import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { getAccountsQueryKey } from "./useGetAccountsQuery.hook";
import type { IHttpUserJson } from "../http-clients/users-http-client";

export function useDeleteAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async (
      {
        concurrencyToken,
        userKey,
      }: {
        userKey: string;
        concurrencyToken: string;
      },
      context,
    ) => {
      const result = await usersClient.deleteAccount(userKey, concurrencyToken);
      const oldData =
        context.client.getQueryData<IHttpUserJson[]>(getAccountsQueryKey);
      if (oldData) {
        context.client.setQueryData(
          getAccountsQueryKey,
          oldData.filter((b) => b.userKey !== userKey),
        );
      }
      return result;
    },
  });
}
