import { Link, useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../../../route-utils";
import type { IRecipeBookModel } from "../../../../services/recipe-types";
import styles from "./table-row.module.css";
import { makeBookNameAndDescriptionState } from "../../../../components/book-information-banner/book-information-banner";

export function TableRow({ book }: { book: IRecipeBookModel }) {
  const navigate = useNavigate();
  const path = makeViewRecipeBookPath(book.id);
  const state = makeBookNameAndDescriptionState(
    book.name,
    book.shortDescription,
  );
  return (
    <tr
      className={`${styles.bookCell} ${styles.bookRow}`}
      onClick={() => navigate(path)}
    >
      <td className={`${styles.bookCell} ${styles.bookName}`}>{book.name}</td>
      <td
        className={`${styles.bookCell} ${styles.bookDescription}`}
        aria-description={book.shortDescription ? undefined : "no description"}
      >
        {book.shortDescription || <>&mdash;</>}
      </td>
      <td className={`${styles.bookCell} ${styles.bookIconLink}`}>
        <Link
          to={path}
          aria-description="navigate to recipe book"
          state={state}
        >
          <span className={styles.emptyEye}>
            <i className="bi bi-eye"></i>
          </span>
          <span className={styles.filledEye}>
            <i className="bi bi-eye-fill"></i>
          </span>
        </Link>
      </td>
    </tr>
  );
}
