import { AccountSelector } from "../../components/accounts-selector/account-selector.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";

/** page for selecting an account */
export function SelectAccountPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <AccountSelector redirectToSignUpIfNeeded />
    </AuthenticatedRouteGuard>
  );
}
