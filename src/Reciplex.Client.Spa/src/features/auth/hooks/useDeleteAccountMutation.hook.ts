import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { maybeUpdateUserListCache } from "../utils/auth-query-cache-utils";

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
      maybeUpdateUserListCache(context.client, (prev) =>
        prev.filter((b) => b.userKey !== userKey),
      );
      return result;
    },
  });
}
