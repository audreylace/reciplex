import { useParams } from "react-router";
import { AccountSettingsPageBody } from "./account-settings-page-body.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** page for modifying the settings for an account */
export function AccountSettingsPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <AccountNotFoundAlert />;
  }

  return (
    <AuthenticatedRouteGuard allowNullUser>
      <BrowserTitle title="Account Settings" />
      <AccountSettingsPageBody userKey={accountKey} key={accountKey} />
    </AuthenticatedRouteGuard>
  );
}
