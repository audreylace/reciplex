import { useParams } from "react-router";
import { DeleteAccountPageBody } from "./delete-account-page-body.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** page for deleting an account */
export function DeleteAccountPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <AccountNotFoundAlert />;
  }

  return (
    <AuthenticatedRouteGuard allowNullUser>
      <BrowserTitle title="Delete Account" />
      <DeleteAccountPageBody userKey={accountKey} key={accountKey} />
    </AuthenticatedRouteGuard>
  );
}
