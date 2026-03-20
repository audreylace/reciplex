import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "preact/hooks";
import { useAuthClients } from "./useAuthClients.hook";
import { getPersistedActiveUser, useActiveUser } from "./useActiveUser.hook";
import { challengeQueryKey } from "./useNeedChallengeQuery.hook";
import { getAccountsQueryKey } from "./useGetAccountsQuery.hook";

export function useRehydrateActiveUser() {
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const setChallengeStatus = useActiveUser((s) => s.setChallengeStatus);
  const isSynced = useActiveUser((s) => s.synced);

  const queryClient = useQueryClient();
  const { challengeClient, usersClient } = useAuthClients();

  useEffect(() => {
    let effectAborted = false;

    if (isSynced) {
      return;
    }

    (async () => {
      const challengeRequired = await queryClient.fetchQuery({
        queryKey: challengeQueryKey,
        queryFn: () => {
          return challengeClient.challengeRequired();
        },
      });

      if (effectAborted) {
        return;
      }

      if (challengeRequired) {
        setChallengeStatus(true);
        return;
      }

      const accounts = await queryClient.fetchQuery({
        queryKey: getAccountsQueryKey,
        queryFn: () => {
          return usersClient.getAccounts();
        },
      });

      if (effectAborted) {
        return;
      }

      const pastAccountId = getPersistedActiveUser();
      const accountIndex = accounts.findIndex(
        (a) => a.userKey === pastAccountId,
      );

      if (accountIndex !== -1 && pastAccountId) {
        setActiveUser({
          userKey: pastAccountId,
          displayName: accounts[accountIndex].displayName,
        });
      } else {
        setActiveUser();
      }
    })();

    return () => {
      effectAborted = true;
    };
  }, [
    challengeClient,
    isSynced,
    queryClient,
    setActiveUser,
    setChallengeStatus,
    usersClient,
  ]);
}
