import { useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { AcceptBookInvitePageBody } from "./accept-book-invite-page-body.component";
import { AuthenticatedRouteGuard } from "../../../auth/components/authenticated-route-guard/authenticated-route-guard.component";

/** page for accepting invites, managing the status of an invite, and leaving a recipe book. */
export function AcceptBookInvitePage() {
  const { bookId, shareKey } = useParams<{
    bookId: string;
    shareKey: string;
  }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <AuthenticatedRouteGuard>
      <AcceptBookInvitePageBody bookId={bookId} shareKey={shareKey} />
    </AuthenticatedRouteGuard>
  );
}
