import { useCallback, useLayoutEffect, useRef, useState } from "preact/hooks";
import {
  useQueryClient,
  type QueryKey,
  type UseQueryResult,
} from "@tanstack/react-query";

/**
 * Hook for loading data that is about to be modified with handling for optimistic concurrency
 */
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
  const cbRef = useRef<() => void | undefined>();
  const queryKeyRef = useRef<QueryKey | undefined>();

  // store deps in ref to avoid regenerating resetState callback
  useLayoutEffect(() => {
    cbRef.current = onReset;
    queryKeyRef.current = queryKey;
  });

  // reset the form by clearing the react-query cache
  const resetState = useCallback(() => {
    cbRef.current?.();
    if (queryKeyRef.current) {
      queryClient.resetQueries({ queryKey: queryKeyRef.current });
    }
    setConcurrencyToken(null);
    setResetCount((k) => k + 1);
  }, [queryClient]);

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
/** args for `useMutationFormState` */
export interface IUseMutationFormStateArgs<TData> {
  /** method invoked to derive the concurrency token */
  concurrencyTokenProvider: (data: NonNullable<TData>) => string | null;
  /** data load query  */
  query: UseQueryResult<TData>;
  /** callback invoked when the main reset callback is invoked */
  onReset?: () => void;
  /** query key of `query`. `resetState` clears this cached data. */
  queryKey: QueryKey;
}
