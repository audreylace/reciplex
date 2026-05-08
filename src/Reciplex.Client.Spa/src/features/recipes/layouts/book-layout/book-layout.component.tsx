import { Outlet, useParams } from "react-router";
import { AuthenticatedRouteGuard } from "../../../auth/components/authenticated-route-guard/authenticated-route-guard.component";
import { BookLayoutNavigationBar } from "../../components/book-layout-navigation-bar/book-layout-navigation-bar.component";

export function BookLayout() {
  const { bookId, recipeId } = useParams<{
    bookId?: string;
    recipeId?: string;
  }>();
  return (
    <>
      <BookLayoutNavigationBar bookId={bookId} recipeId={recipeId} />
      <main className="pageMain">
        <AuthenticatedRouteGuard>
          <Outlet />
        </AuthenticatedRouteGuard>
      </main>
    </>
  );
}
