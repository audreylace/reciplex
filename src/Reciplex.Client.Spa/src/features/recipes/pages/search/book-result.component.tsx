import { purple } from "@mui/material/colors";
import { makeViewRecipeBookPath } from "../../route-utils";
import { ResultCard } from "./result-card.component";

/**
 * Renders a book result
 */
export function BookResult({
  name,
  shortDescription,
  bookId,
}: IBookResultsProps) {
  return (
    <ResultCard
      name={name}
      description={shortDescription}
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
   * the id of the book
   */
  bookId: string;
}
