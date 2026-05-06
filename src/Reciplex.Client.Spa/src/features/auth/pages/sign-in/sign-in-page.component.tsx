import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { RedirectOnRender } from "../../components/redirect-on-render/redirect-on-render.component";

/** Page that either shows the challenge UI or redirects the user to the account select page */
export function SignInPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <RedirectOnRender />
    </AuthenticatedRouteGuard>
  );
}
