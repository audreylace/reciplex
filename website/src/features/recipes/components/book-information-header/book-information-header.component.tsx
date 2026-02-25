import { useLocation } from "react-router";
import styles from "./book-information-header.module.css";

/** Information about a book */
export function BookInformationHeader({
  name,
  shortDescription,
}: BookInformationHeaderProps) {
  const location = useLocation();
  const { bookName, bookShortDescription } = location?.state ?? {};
  const finalName = name ?? bookName;
  const finalShortDescription = shortDescription ?? bookShortDescription;

  if (!finalName && !finalShortDescription) {
    return null;
  }
  return (
    <div className={styles.header}>
      <h1>{name}</h1>
      <p>{shortDescription}</p>
    </div>
  );
}

/** props for `BookInformationHeader` */
export interface BookInformationHeaderProps {
  /** book name */
  name?: string | null;
  /** book short description */
  shortDescription?: string | null;
}

/**
 * Creates an object that can be merged into the
 * location state array holding a book name and description.
 * Used by `BookInformationBanner` to show identifying book
 * information while the book data is loading.
 */
export function makeBookNameAndDescriptionState(
  bookName?: string,
  bookShortDescription?: string,
) {
  return {
    bookName,
    bookShortDescription,
  };
}
