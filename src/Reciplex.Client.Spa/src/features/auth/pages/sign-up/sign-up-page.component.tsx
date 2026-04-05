import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { SignUpForm } from "../../components/sign-up-form/sign-up-form.component";

/** page for making an account */
export function SignUpPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <SignUpForm />
    </AuthenticatedRouteGuard>
  );
}
