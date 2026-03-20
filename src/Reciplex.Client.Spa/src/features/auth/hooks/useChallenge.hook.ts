import { useState, useEffect } from "preact/hooks";
import { useAuthClients } from "./useAuthClients.hook";

export function useChallenge() {
  const { challengeClient } = useAuthClients();
  const [challenge, setChallenge] = useState(false);
  useEffect(() => {
    if (challenge) {
      setChallenge(false);
      challengeClient.beginChallenge();
    }
  }, [challenge, challengeClient]);

  return () => setChallenge(true);
}
