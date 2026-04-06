import { NavLink, Outlet, useParams } from "react-router";
import styles from "./book-layout.module.css";
import { AppNavigation } from "../../../core/components/app-navigation/app-navigation.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import {
  makeBookListPath,
  makeViewRecipeBookPath,
  makeViewRecipePath,
} from "../../route-utils";
import { AuthenticatedRouteGuard } from "../../../auth/components/authenticated-route-guard/authenticated-route-guard.component";

export function BookLayout() {
  return (
    <AuthenticatedRouteGuard>
      <BookLayoutBar />
      <main className="pageMain">
        <Outlet />
      </main>
    </AuthenticatedRouteGuard>
  );
}

function BookLayoutBar() {
  const { bookId, recipeId } = useParams<{
    bookId?: string;
    recipeId?: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId);
  return (
    <AppNavigation childrenType="list-elements">
      <ul className={styles.crumbTrail}>
        <li className={styles.crumbElement}>
          <NavLink to={makeBookListPath()}>
            <SuccessButton buttonType="hidden">
              <span className={styles.bookCrumb}>Books</span>
            </SuccessButton>
          </NavLink>
        </li>
        {bookId && (
          <>
            <CrumbDivider />
            <li className={styles.crumbElement}>
              <NavLink to={makeViewRecipeBookPath(bookId ?? "")}>
                <SuccessButton buttonType="hidden">
                  <span className={styles.bookCrumb}>
                    {bookQuery.data?.name ?? ""}
                  </span>
                </SuccessButton>
              </NavLink>
            </li>
          </>
        )}
        {recipeId && bookId && recipeQuery.data && (
          <>
            <CrumbDivider />
            <li className={styles.crumbElement}>
              <NavLink to={makeViewRecipePath(bookId, recipeId)}>
                <SuccessButton buttonType="hidden">
                  <span className={styles.recipeCrumb}>
                    {recipeQuery.data?.name ?? ""}
                  </span>
                </SuccessButton>
              </NavLink>
            </li>
          </>
        )}
      </ul>
    </AppNavigation>
  );
}

function CrumbDivider() {
  return <li className={styles.crumbDivider}>/</li>;
}
