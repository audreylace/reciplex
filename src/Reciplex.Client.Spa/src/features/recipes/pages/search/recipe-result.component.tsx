import { red } from "@mui/material/colors";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import { ResultCard } from "./result-card.component";
import { NavLink } from "react-router";
import style from "./recipe-result.module.css";

/**
 * Renders a recipe result
 */
export function RecipeResult({
  name,
  shortDescription,
  shared,
  bookId,
  recipeId,
  bookTitle,
}: IRecipeResultsProps) {
  return (
    <ResultCard
      name={name}
      description={shortDescription}
      shared={shared}
      avatarColor={red[500]}
      avatarLetter="R"
      caption={
        <NavLink to={makeViewRecipeBookPath(bookId)} className={style.link}>
          Book / {bookTitle}
        </NavLink>
      }
      path={makeViewRecipePath(bookId, recipeId)}
    />
  );
}

/**
 * Properties for the `<RecipeResult />` component
 */
export interface IRecipeResultsProps {
  /**
   * name of the recipe
   */
  name: string;
  /**
   * recipe short description
   */
  shortDescription: string;
  /**
   * flag for if the recipe is shared, not owned by the user
   */
  shared?: boolean;
  /**
   * the id of the book holding the recipe
   */
  bookId: string;
  /**
   * the id of the recipe
   */
  recipeId: string;
  /**
   * title of the book
   */
  bookTitle: string;
}
