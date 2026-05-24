import { useMemo } from "react";
import { useGetAccountsQuery } from "./useGetAccountsQuery.hook";
import { useMutationFormState } from "../../core/hooks/useMutationFormState.hook";
import { accountListAuthQueryKey } from "../utils/auth-query-key-factory";

export function useAccountMutationState(userKey: string) {
  const accountsQuery = useGetAccountsQuery(undefined, { alwaysFresh: true });

  const { concurrencyToken, resetState, resetCount, concurrencyConflict } =
    useMutationFormState({
      queryKey: accountListAuthQueryKey(),
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
