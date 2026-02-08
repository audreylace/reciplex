import { Link, useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../../../route-utils";
import type { IRecipeBookModel } from "../../../../../../services/recipe-store";
import styles from "./table-row.module.css";

export function TableRow({ book }: { book: IRecipeBookModel }) {
  const navigate = useNavigate();
  const path = makeViewRecipeBookPath(book.id);
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
        <Link to={path} aria-description="navigate to recipe book">
          <span className={styles.emptyEye}>
            <i class="bi bi-eye"></i>
          </span>
          <span className={styles.filledEye}>
            <i class="bi bi-eye-fill"></i>
          </span>
        </Link>
      </td>
    </tr>
  );
}
