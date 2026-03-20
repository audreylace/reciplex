import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

export function useCreateAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async ({ displayName }: { displayName: string }) => {
      return await usersClient.createAccount({ displayName });
    },
  });
}
