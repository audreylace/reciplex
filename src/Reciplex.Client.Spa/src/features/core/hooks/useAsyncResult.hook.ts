import {
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from "preact/hooks";

export function useAsyncResult<T>({
  asyncAction,
  disabled,
}: IUseAsyncResultArgs<T>): UseAsyncResultReturn<T> {
  const asyncCallbackRef = useRef<() => Promise<T>>(asyncAction);
  const [loadingState, setLoadingState] = useState<HookState<T>>({
    state: "pending",
  });

  useLayoutEffect(() => {
    asyncCallbackRef.current = asyncAction;
  });

  useEffect(() => {
    if (disabled || loadingState.state !== "pending") {
      return;
    }
    let mounted = true;

    asyncCallbackRef
      .current()
      .then((value: T) => {
        if (!mounted) {
          return;
        }

        setLoadingState((prev) => {
          if (prev !== loadingState) {
            return prev;
          }
          return {
            state: "success",
            data: value,
            error: null,
          };
        });
      })
      .catch((error: unknown) => {
        if (!mounted) {
          return;
        }
        setLoadingState((prev) => {
          if (prev !== loadingState) {
            return prev;
          }
          return {
            state: "error",
            data: null,
            error: error,
          };
        });
      });

    return () => {
      mounted = false;
    };
  }, [disabled, loadingState]);

  const reset = useCallback(() => {
    setLoadingState({
      state: "pending",
    });
  }, []);

  if (disabled) {
    return {
      state: "disabled",
      reset,
    };
  }

  switch (loadingState.state) {
    case "pending":
      return {
        state: "pending",
        reset,
      };
    case "success":
      return {
        state: "success",
        data: loadingState.data,
        reset,
      };
    case "error":
      return {
        state: "error",
        error: loadingState.error,
        reset,
      };
    default:
      return {
        state: "error",
        error: Error(),
        reset,
      };
  }
}

export interface IUseAsyncResultArgs<T> {
  asyncAction: () => Promise<T>;
  disabled?: boolean;
}

type UseAsyncResultReturn<T> =
  | {
      state: "pending";
      reset: () => void;
    }
  | {
      state: "success";
      reset: () => void;
      data: T;
    }
  | {
      state: "error";
      reset: () => void;
      error: unknown;
    }
  | {
      state: "disabled";
      reset: () => void;
    };

type HookState<T> =
  | { state: "success"; data: T }
  | { state: "error"; error: unknown }
  | { state: "pending" };
