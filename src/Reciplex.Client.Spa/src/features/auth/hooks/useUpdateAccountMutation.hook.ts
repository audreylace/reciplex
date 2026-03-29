import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

export function useUpdateAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async ({
      displayName,
      concurrencyToken,
      userKey,
    }: {
      userKey: string;
      displayName: string;
      concurrencyToken: string;
    }) => {
      return await usersClient.updateAccount(
        userKey,
        { displayName },
        concurrencyToken,
      );
    },
  });
}
