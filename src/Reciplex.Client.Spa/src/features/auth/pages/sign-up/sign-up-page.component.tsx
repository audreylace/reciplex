import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { SignUpForm } from "../../components/sign-up-form/sign-up-form.component";

export function SignUpPage() {
  return (
    <AuthenticatedRouteGuard>
      <SignUpForm />
    </AuthenticatedRouteGuard>
  );
}
