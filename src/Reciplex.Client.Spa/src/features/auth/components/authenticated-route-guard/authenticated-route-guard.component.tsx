import type { ComponentChildren } from "preact";
import { ChallengeGuard } from "../challenge-guard/challenge-guard.component";
import { ActiveUserGuard } from "../active-user-guard/active-user-guard.component";

/** Guards a route making sure a challenge is not needed and an account is selected */
export function AuthenticatedRouteGuard({
  children,
}: {
  children: ComponentChildren;
}) {
  return (
    <ChallengeGuard>
      <ActiveUserGuard>{children}</ActiveUserGuard>
    </ChallengeGuard>
  );
}
