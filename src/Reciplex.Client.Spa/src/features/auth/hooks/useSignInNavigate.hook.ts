import { useCallback } from "preact/hooks";
import { useNavigate } from "react-router";

/** path to the sign up page */
const signUpPath = "/accounts/-/sign-in";

/**
 * Returns the path to the sign in page and a
 * delegate that when invoked triggers a
 * navigation to that page.
 * @returns tuple where the arg0 is the
 * path and arg1 is the navigate action.
 * Bind arg0 to href and arg1 to the event handler.
 */
export function useSignInNavigate(): [
  string,
  (e?: Event | null | undefined) => void,
] {
  const navigate = useNavigate();
  const navigateAction = useCallback(
    (e: Event | null | undefined) => {
      e?.preventDefault();
      navigate(signUpPath);
    },
    [navigate],
  );

  return [signUpPath, navigateAction];
}
