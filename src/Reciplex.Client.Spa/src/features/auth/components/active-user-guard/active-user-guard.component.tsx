import type { ComponentChildren } from "preact";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { useActiveUserGuard } from "../../hooks/useActiveUserGuard.hook";

/**
 * Redirects the user to the select account if a account has not yet been selected.
 * Assumes user is authenticated.
 */
export function ActiveUserGuard({
  children,
  allowNullUser,
}: {
  allowNullUser?: boolean;
  children: ComponentChildren;
}) {
  const { renderChildren, status, retry } = useActiveUserGuard(allowNullUser);

  if (status === "pending") {
    return <LoadingIndicator />;
  }

  if (status === "error") {
    return <LoadingFailedAlert onRetry={retry} />;
  }

  if (!renderChildren) {
    return null;
  }

  return children;
}
