import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { maybeUpdateUserListCache } from "../utils/auth-query-cache-utils";

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

      maybeUpdateUserListCache(context.client, (prev) => {
        const accountIndex = prev.findIndex(
          (a) => a.userKey === result.userKey,
        );
        if (accountIndex !== -1) {
          const newData = prev.slice(0);
          newData.splice(accountIndex, 1, result);
          return newData;
        }

        return prev;
      });

      return result;
    },
  });
}
