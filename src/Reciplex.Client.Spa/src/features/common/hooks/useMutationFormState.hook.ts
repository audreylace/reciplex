import { useCallback, useState } from "preact/hooks";
import {
  useQueryClient,
  type QueryKey,
  type UseQueryResult,
} from "@tanstack/react-query";

export function useMutationFormState<TData>({
  concurrencyTokenProvider,
  query,
  onReset,
  queryKey,
}: IUseMutationFormStateArgs<TData>) {
  const queryClient = useQueryClient();

  const [concurrencyToken, setConcurrencyToken] = useState<string | null>();
  // component key - incremented each time we reset the form
  // to recycle the component
  const [resetCount, setResetCount] = useState(1);

  // reset the form by clearing the react-query cache
  const resetState = useCallback(() => {
    onReset?.();
    queryClient.resetQueries({ queryKey: queryKey });
    setConcurrencyToken(null);
    setResetCount((k) => k + 1);
  }, [onReset, queryClient, queryKey]);

  // lock concurrency token to detect concurrent modifications
  setConcurrencyToken((token) => {
    if (token || !query.isSuccess || !query.data) {
      return token;
    }

    return concurrencyTokenProvider(query.data);
  });

  const concurrencyConflict = !!(
    query.data &&
    concurrencyToken &&
    concurrencyToken !== concurrencyTokenProvider(query.data)
  );

  return {
    resetState: resetState,
    concurrencyConflict: concurrencyConflict,
    concurrencyToken: concurrencyToken,
    resetCount: resetCount,
  };
}
export interface IUseMutationFormStateArgs<TData> {
  concurrencyTokenProvider: (data: NonNullable<TData>) => string | null;
  query: UseQueryResult<TData>;
  onReset?: () => void;
  queryKey: QueryKey;
}
