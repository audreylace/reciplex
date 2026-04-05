import { AccountSelector } from "../../components/accounts-selector/account-selector.component";
import { ChallengeGuard } from "../../components/challenge-guard/challenge-guard.component";

/** page for selecting an account */
export function SelectAccountPage() {
  return (
    <ChallengeGuard>
      <AccountSelector redirectToSignUpIfNeeded />
    </ChallengeGuard>
  );
}
