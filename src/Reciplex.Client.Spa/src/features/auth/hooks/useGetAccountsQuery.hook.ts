import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { useCallback } from "preact/hooks";

export const getAccountsQueryKey = ["auth", "getAccounts"];
export function useGetAccountsQuery(
  enable?: boolean,
  args?: {
    noCache?: boolean;
  },
) {
  const { usersClient } = useAuthClients();
  return useQuery({
    enabled: enable,
    gcTime: args?.noCache ? 0 : undefined,
    staleTime: args?.noCache ? 0 : 60 * 1000,
    queryKey: getAccountsQueryKey,
    queryFn: async () => {
      return await usersClient.getAccounts();
    },
  });
}

/** creates a callback that may be used to fully reset the `useGetAccountsQuery` */
export function useResetAccountsQuery() {
  const queryClient = useQueryClient();
  return useCallback(() => {
    queryClient.resetQueries({ queryKey: getAccountsQueryKey });
  }, [queryClient]);
}
