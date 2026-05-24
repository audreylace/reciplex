import { useCallback, useLayoutEffect, useRef, useState } from "react";
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
  const cbRef = useRef<() => void | undefined>(undefined);
  const queryKeyRef = useRef<QueryKey | undefined>(undefined);

  if (!concurrencyToken && query.isSuccess && query.data) {
    const token = concurrencyTokenProvider(query.data);
    // lock concurrency token to detect concurrent modifications
    setConcurrencyToken(token);
  }

  // store deps in ref to avoid regenerating resetState callback
  useLayoutEffect(() => {
    cbRef.current = onReset;
    queryKeyRef.current = queryKey;
  }, [
    onReset,
    queryKey,
    query.isSuccess,
    query.data,
    concurrencyTokenProvider,
  ]);

  // reset the form by clearing the react-query cache
  const resetState = useCallback(() => {
    cbRef.current?.();
    if (queryKeyRef.current) {
      queryClient.resetQueries({ queryKey: queryKeyRef.current });
    }
    setConcurrencyToken(null);
    setResetCount((k) => k + 1);
  }, [queryClient]);

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
