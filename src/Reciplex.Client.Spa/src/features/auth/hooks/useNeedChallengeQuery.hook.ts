import { useQuery } from "@tanstack/react-query";
import { useAuthClients } from "./useAuthClients.hook";

export const challengeQueryKey = ["auth", "challenge-needed"];
export function useNeedChallengeQuery() {
  const { challengeClient } = useAuthClients();
  return useQuery({
    queryKey: challengeQueryKey,
    staleTime: 60 * 1000,
    queryFn: async () => {
      return await challengeClient.challengeRequired();
    },
  });
}
