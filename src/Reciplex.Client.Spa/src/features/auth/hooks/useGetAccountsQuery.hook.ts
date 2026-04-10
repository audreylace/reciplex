import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { useCallback } from "preact/hooks";
import { accountListAuthQueryKey } from "../utils/auth-query-key-factory";

export function useGetAccountsQuery(
  enable?: boolean,
  args?: {
    alwaysFresh?: boolean;
  },
) {
  const { usersClient } = useAuthClients();
  return useQuery({
    enabled: enable,
    refetchOnMount: args?.alwaysFresh ? "always" : true,
    queryKey: accountListAuthQueryKey(),
    queryFn: async () => {
      return await usersClient.getAccounts();
    },
  });
}

/** creates a callback that may be used to fully reset the `useGetAccountsQuery` */
export function useResetAccountsQuery() {
  const queryClient = useQueryClient();
  return useCallback(() => {
    queryClient.resetQueries({ queryKey: accountListAuthQueryKey() });
  }, [queryClient]);
}
