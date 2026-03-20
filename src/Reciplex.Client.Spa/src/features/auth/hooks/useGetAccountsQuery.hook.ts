import { useQuery } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

export const getAccountsQueryKey = ["auth", "getAccounts"];
export function useGetAccountsQuery(enable?: boolean) {
  const { usersClient } = useAuthClients();
  return useQuery({
    enabled: enable,
    queryKey: getAccountsQueryKey,
    queryFn: async () => {
      return await usersClient.getAccounts();
    },
  });
}
