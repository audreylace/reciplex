import { useCallback } from "preact/hooks";
import { useNavigate } from "react-router";

/**
 * Returns the path to the setting page for the provided account and a
 * delegate that when invoked triggers a
 * navigation to that page.
 * @param userKey the key of the account
 * @returns tuple where the arg0 is the
 * path and arg1 is the navigate action.
 * Bind arg0 to href and arg1 to the event handler.
 */
export function useAccountDeletesNavigate(
  userKey: string,
): [string, () => void] {
  const navigate = useNavigate();
  const path = `/accounts/${encodeURIComponent(userKey)}/delete`;

  const action = useCallback(() => {
    navigate(path);
  }, [navigate, path]);

  return [path, action];
}
