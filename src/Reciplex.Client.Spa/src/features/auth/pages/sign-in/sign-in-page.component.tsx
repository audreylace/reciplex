import { useEffect } from "preact/hooks";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";

/** Page that either shows the challenge UI or redirects the user to the account select page */
export function SignInPage() {
  return (
    <AuthenticatedRouteGuard allowNullUser>
      <Redirect />
    </AuthenticatedRouteGuard>
  );
}

/** Component that when rendered goes to the account select page */
function Redirect() {
  const goToSelect = useSelectAccountNavigate()[1];

  useEffect(() => {
    goToSelect({
      replace: true, // back button should go back to the page that summoned us
    });
  }, [goToSelect]);

  return null;
}
