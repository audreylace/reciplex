import { useMutation } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { maybeUpdateUserListCache } from "../utils/auth-query-cache-utils";

export function useCreateAccountMutation() {
  const { usersClient } = useAuthClients();

  return useMutation({
    mutationFn: async ({ displayName }: { displayName: string }, context) => {
      const result = await usersClient.createAccount({ displayName });

      maybeUpdateUserListCache(context.client, (prev) => {
        if (!prev.find((a) => a.userKey === result.userKey)) {
          return [...prev, result];
        }
        return prev;
      });

      return result;
    },
  });
}
