import { NavLink } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import {
  makeBookListPath,
  makeViewRecipeBookPath,
  makeViewRecipePath,
} from "../../route-utils";
import Typography from "@mui/material/Typography";
import bookLayoutCrumbsStyleModule from "./book-layout-crumbs.module.css";
import { CrumbDivider } from "./crumb-divider.component";

export function BookLayoutCrumbs({ bookId, recipeId }: IBookLayoutCrumbsProps) {
  const bookQuery = useGetRecipeBookById(bookId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId);

  return (
    <ul className={bookLayoutCrumbsStyleModule.crumbTrail}>
      <li className={bookLayoutCrumbsStyleModule.crumbElement}>
        <NavLink to={makeBookListPath()}>
          <span className={bookLayoutCrumbsStyleModule.bookCrumb}>
            <Typography variant="subtitle1">Books</Typography>
          </span>
        </NavLink>
      </li>
      {bookId && (
        <>
          <CrumbDivider />
          <li className={bookLayoutCrumbsStyleModule.crumbElement}>
            <NavLink to={makeViewRecipeBookPath(bookId ?? "")}>
              <span className={bookLayoutCrumbsStyleModule.bookCrumb}>
                <Typography variant="subtitle1">
                  {bookQuery.data?.name ?? ""}
                </Typography>
              </span>
            </NavLink>
          </li>
        </>
      )}
      {recipeId && bookId && recipeQuery.data && (
        <>
          <CrumbDivider />
          <li className={bookLayoutCrumbsStyleModule.crumbElement}>
            <NavLink to={makeViewRecipePath(bookId, recipeId)}>
              <span className={bookLayoutCrumbsStyleModule.recipeCrumb}>
                <Typography variant="subtitle1">
                  {recipeQuery.data?.name ?? ""}
                </Typography>
              </span>
            </NavLink>
          </li>
        </>
      )}
    </ul>
  );
}

export interface IBookLayoutCrumbsProps {
  bookId?: string;
  recipeId?: string;
}
