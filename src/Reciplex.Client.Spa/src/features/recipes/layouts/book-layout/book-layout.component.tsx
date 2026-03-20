import { NavLink, Outlet, useNavigate, useParams } from "react-router";
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
import { useActiveUser } from "../../../auth/hooks/useActiveUser.hook";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { useEffect } from "preact/hooks";
import { useRehydrateActiveUser } from "../../../auth/hooks/useRehydrateActiveUser.hook";

export function BookLayout() {
  useRehydrateActiveUser();

  const isSynced = useActiveUser((s) => s.synced);
  const challengeNeeded = useActiveUser((s) => s.challengeNeeded);
  const userKey = useActiveUser((s) => s.userKey);

  const navigate = useNavigate();

  useEffect(() => {
    if (isSynced && (!userKey || challengeNeeded)) {
      navigate("/sign-in", {
        state: {
          redirect: window.location.href,
        },
      });
    }
  }, [navigate, isSynced, userKey, challengeNeeded]);

  if (!isSynced) {
    return (
      <InformationBanner title="Checking Sign-In status"></InformationBanner>
    );
  }
  if (challengeNeeded || !userKey) {
    return <InformationBanner title="Sign-In Needed"></InformationBanner>;
  }

  return (
    <>
      <BookLayoutBar />
      <Outlet />
    </>
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
            <SuccessButton buttonType="hidden">Books</SuccessButton>
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
