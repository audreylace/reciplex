import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { doUserCacheUpdate } from "../utils/auth-query-cache-utils";

export function useCreateAccountMutation() {
  const { usersClient } = useAuthClients();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ displayName }: { displayName: string }) =>
      usersClient.createAccount({ displayName }),
    onSuccess: (data) => doUserCacheUpdate(queryClient, data),
  });
}
