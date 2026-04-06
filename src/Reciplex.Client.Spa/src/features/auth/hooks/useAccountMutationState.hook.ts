import { useMemo } from "preact/hooks";
import {
  getAccountsQueryKey,
  useGetAccountsQuery,
} from "./useGetAccountsQuery.hook";
import { useMutationFormState } from "../../common/hooks/useMutationFormState.hook";

export function useAccountMutationState(userKey: string) {
  const accountsQuery = useGetAccountsQuery(undefined, { noCache: true });

  const { concurrencyToken, resetState, resetCount, concurrencyConflict } =
    useMutationFormState({
      queryKey: getAccountsQueryKey,
      query: accountsQuery,
      concurrencyTokenProvider: (data) => {
        return (
          data?.find((acc) => acc.userKey === userKey)?.concurrencyTag ?? null
        );
      },
    });

  // extract account from the list
  const account = useMemo(() => {
    if (accountsQuery.isSuccess) {
      return accountsQuery.data?.find((acc) => acc.userKey === userKey) ?? null;
    }
    return null;
  }, [accountsQuery.data, accountsQuery.isSuccess, userKey]);

  return {
    resetState: resetState,
    concurrencyConflict: concurrencyConflict,
    query: accountsQuery,
    concurrencyToken: concurrencyToken,
    account: account,
    resetCount: resetCount,
  };
}
