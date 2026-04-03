import { AccountSelector } from "../../components/accounts-selector/account-selector.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";

export function SelectAccountPage() {
  return (
    <AuthenticatedRouteGuard>
      <AccountSelector />
    </AuthenticatedRouteGuard>
  );
}
