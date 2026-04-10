import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { maybeUpdateUserListCache } from "../utils/auth-query-cache-utils";

export function useDeleteAccountMutation() {
  const { usersClient } = useAuthClients();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      concurrencyToken,
      userKey,
    }: {
      userKey: string;
      concurrencyToken: string;
    }) => usersClient.deleteAccount(userKey, concurrencyToken),
    onSuccess: (_, { userKey }) => {
      maybeUpdateUserListCache(queryClient, (prev) =>
        prev.filter((b) => b.userKey !== userKey),
      );
    },
  });
}
