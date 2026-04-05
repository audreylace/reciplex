import { useQuery } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

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
    staleTime: args?.noCache ? 0 : undefined,
    queryKey: getAccountsQueryKey,
    queryFn: async () => {
      return await usersClient.getAccounts();
    },
  });
}
