import { ChallengeGuard } from "../../components/challenge-guard/challenge-guard.component";
import { useParams } from "react-router";
import { DeleteAccountPageBody } from "./delete-account-page-body.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";

/** page for deleting an account */
export function DeleteAccountPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <AccountNotFoundAlert />;
  }

  return (
    <ChallengeGuard>
      <DeleteAccountPageBody userKey={accountKey} />
    </ChallengeGuard>
  );
}
