import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { SignOutPageBody } from "./sign-out-page-body.component";

/** Page that either shows the challenge UI or redirects the user to the account select page */
export function SignOutPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <SignOutPageBody />
    </AuthenticatedRouteGuard>
  );
}
