import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

export function useDeleteAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async ({
      concurrencyToken,
      userKey,
    }: {
      userKey: string;
      concurrencyToken: string;
    }) => {
      return await usersClient.deleteAccount(userKey, concurrencyToken);
    },
  });
}
