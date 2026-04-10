import { useQuery } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";
import { challengeAuthQueryKey } from "../utils/auth-query-key-factory";

export function useNeedChallengeQuery() {
  const { challengeClient } = useAuthClients();
  return useQuery({
    queryKey: challengeAuthQueryKey(),
    queryFn: async () => {
      return await challengeClient.challengeRequired();
    },
  });
}
