import { useCallback, useMemo, useState } from "preact/hooks";
import {
  useGetAccountsQuery,
  useResetAccountsQuery,
} from "./useGetAccountsQuery.hook";

export function useAccountMutationState(userKey: string) {
  const resetAccountsQuery = useResetAccountsQuery();
  const accountsQuery = useGetAccountsQuery(undefined, { noCache: true });
  const [concurrencyToken, setConcurrencyToken] = useState<string | null>();
  // component key - incremented each time we reset the form
  // to recycle the component
  const [resetCount, setResetCount] = useState(1);

  // extract account from the list
  const account = useMemo(() => {
    if (accountsQuery.isSuccess) {
      return accountsQuery.data?.find((acc) => acc.userKey === userKey) ?? null;
    }
    return null;
  }, [accountsQuery.data, accountsQuery.isSuccess, userKey]);

  // reset the form by clearing the react-query cache
  const onReset = useCallback(() => {
    resetAccountsQuery();
    setConcurrencyToken(null);
    setResetCount((k) => k + 1);
  }, [resetAccountsQuery]);

  // lock concurrency token to detect concurrent modifications
  setConcurrencyToken((token) => {
    if (!account || token) {
      return token;
    }
    return account.concurrencyTag;
  });

  const concurrencyConflict = !!(
    concurrencyToken &&
    account &&
    concurrencyToken !== account.concurrencyTag
  );

  return {
    resetState: onReset,
    concurrencyConflict: concurrencyConflict,
    query: accountsQuery,
    concurrencyToken: concurrencyToken,
    account: account,
    resetCount: resetCount,
  };
}
