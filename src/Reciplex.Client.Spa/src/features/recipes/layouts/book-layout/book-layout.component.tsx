import { Outlet, useParams } from "react-router";
import { AuthenticatedRouteGuard } from "../../../auth/components/authenticated-route-guard/authenticated-route-guard.component";
import { BookLayoutNavigationBar } from "../../components/book-layout-navigation-bar/book-layout-navigation-bar.component";
import { NavigationInProgressIndicatorComponent } from "../../../core/components/navigation-in-progress-indicator/navigation-in-progress-indicator.component";

export function BookLayout() {
  const { bookId, recipeId } = useParams<{
    bookId?: string;
    recipeId?: string;
  }>();
  return (
    <>
      <BookLayoutNavigationBar bookId={bookId} recipeId={recipeId} />
      <NavigationInProgressIndicatorComponent />
      <main className="pageMain">
        <AuthenticatedRouteGuard>
          <Outlet />
        </AuthenticatedRouteGuard>
      </main>
    </>
  );
}
