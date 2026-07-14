import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { AccountSelector } from "../../components/accounts-selector/account-selector.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { AccountTopMenuButton } from "../../components/account-top-menu-button/account-top-menu-button.component";

/** page for selecting an account */
export function SelectAccountPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <BrowserTitle title="Select Account" />
      <PageHeader
        title="Whose Cooking?"
        sideComponent={<AccountTopMenuButton />}
      />
      <AccountSelector redirectToSignUpIfNeeded />
    </AuthenticatedRouteGuard>
  );
}
