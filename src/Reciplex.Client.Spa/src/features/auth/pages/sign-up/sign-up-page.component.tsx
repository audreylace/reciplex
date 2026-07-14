import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { AccountTopMenuButton } from "../../components/account-top-menu-button/account-top-menu-button.component";
import { SignUpForm } from "../../components/sign-up-form/sign-up-form.component";

/** page for making an account */
export function SignUpPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <BrowserTitle title="Sign up" />
      <PageHeader title="Welcome!" sideComponent={<AccountTopMenuButton />} />
      <SignUpForm />
    </AuthenticatedRouteGuard>
  );
}
