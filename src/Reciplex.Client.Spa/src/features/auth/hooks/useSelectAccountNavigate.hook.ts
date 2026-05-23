import { useCallback } from "react";
import { useNavigate, type NavigateOptions } from "react-router";

/** the path to the page */
const path = "/accounts/-/select";

/**
 * Hook that returns a tuple. The first
 * value is the path. The second is a delegate
 * that when called will navigate the user
 * to path.
 */
export function useSelectAccountNavigate(): [
  string,
  (options?: NavigateOptions) => void,
] {
  const navigate = useNavigate();

  const callback = useCallback(
    (options?: NavigateOptions) => {
      navigate("/accounts/-/select", options);
    },
    [navigate],
  );
  return [path, callback];
}
