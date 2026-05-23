import { useCallback } from "react";
import { useAuthClients } from "./useAuthClients.hook";

export function useChallenge() {
  const { challengeClient } = useAuthClients();

  return useCallback(() => {
    challengeClient.beginChallenge();
  }, [challengeClient]);
}
