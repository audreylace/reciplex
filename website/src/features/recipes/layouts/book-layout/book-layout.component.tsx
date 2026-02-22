import { NavLink, Outlet, useParams } from "react-router";
import styles from "./book-layout.module.css";
import { AppNavigation } from "../../../core/components/app-navigation/app-navigation.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import {
  makeBookListPath,
  makeViewRecipeBookPath,
  makeViewRecipePath,
} from "../../route-utils";

export function BookLayout() {
  return (
    <>
      <BookLayoutBar />
      <Outlet />
    </>
  );
}

function BookLayoutBar() {
  const { bookId, recipeId } = useParams<{
    bookId: string;
    recipeId: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId);
  return (
    <AppNavigation childrenType="list-elements">
      <ul className={styles.crumbTrail}>
        <li className={styles.crumbElement}>
          <NavLink to={makeBookListPath()}>
            <SuccessButton buttonType="hidden">Books</SuccessButton>
          </NavLink>
        </li>
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
        {recipeId && bookId && recipeQuery.data && (
          <>
            <CrumbDivider />
            <li className={styles.crumbElement}>
              <NavLink to={makeViewRecipePath(bookId, recipeId)}>
                <SuccessButton buttonType="hidden">
                  <span className={styles.recipeCrumb}>
                    {recipeQuery.data?.recipe.name ?? ""}
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
