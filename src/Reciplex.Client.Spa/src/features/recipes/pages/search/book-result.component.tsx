import { purple } from "@mui/material/colors";
import { makeViewRecipeBookPath } from "../../route-utils";
import { ResultCard } from "./result-card.component";

/**
 * Renders a book result
 */
export function BookResult({
  name,
  shortDescription,
  shared,
  bookId,
}: IBookResultsProps) {
  return (
    <ResultCard
      name={name}
      description={shortDescription}
      shared={shared}
      avatarColor={purple[300]}
      avatarLetter="B"
      path={makeViewRecipeBookPath(bookId)}
    />
  );
}

/**
 * Properties for the `<BookResult />` component
 */
export interface IBookResultsProps {
  /**
   * name of the recipe book
   */
  name: string;
  /**
   * recipe book short description
   */
  shortDescription: string;
  /**
   * flag for if the recipe book is shared, not owned by the user
   */
  shared?: boolean;
  /**
   * the id of the book
   */
  bookId: string;
}
