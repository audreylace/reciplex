import { useParams } from "react-router";
import { AccountSettingsPageBody } from "./account-settings-page-body.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";

/** page for modifying the settings for an account */
export function AccountSettingsPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <AccountNotFoundAlert />;
  }

  return (
    <AuthenticatedRouteGuard allowNullUser>
      <AccountSettingsPageBody userKey={accountKey} key={accountKey} />
    </AuthenticatedRouteGuard>
  );
}
