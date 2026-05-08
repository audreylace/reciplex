import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { doUserCacheUpdate } from "../utils/auth-query-cache-utils";

export function useUpdateAccountMutation() {
  const { usersClient } = useAuthClients();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      displayName,
      concurrencyToken,
      userKey,
    }: {
      userKey: string;
      displayName: string;
      concurrencyToken: string;
    }) => usersClient.updateAccount(userKey, { displayName }, concurrencyToken),
    onSuccess: (data) => doUserCacheUpdate(queryClient, data),
  });
}
