import { Link } from "react-router";
import {
  BookListNavigationAction,
  makeBookListPath,
} from "../../../../features/recipes/route-utils";
import styles from "./pagination-bar.module.css";

/** UI for navigating pages both forward and backwards */
export function PaginationBar({
  previousCursor,
  nextCursor,
}: {
  /** Id used to access the previous page. Undefined means there is not a previous page. */
  previousCursor?: string;
  /** Id used to access the next page. Undefined means there is not a next page. */
  nextCursor?: string;
}) {
  return (
    <nav
      aria-label="Page navigation for the list of recipe books"
      className={`${styles.componentWrapper}`}
    >
      <ul className="pagination">
        <li
          className={`page-item ${previousCursor ? "" : "disabled"}`}
          aria-disabled={previousCursor ? false : true}
        >
          <Link className="page-link" to={makeBookListPath()}>
            First
          </Link>
        </li>
        <li
          className={`page-item ${previousCursor ? "" : "disabled"}`}
          aria-disabled={previousCursor ? false : true}
        >
          <Link
            className="page-link"
            to={
              previousCursor
                ? makeBookListPath({
                    cursor: {
                      order: BookListNavigationAction.previous,
                      index: previousCursor,
                    },
                  })
                : "#"
            }
          >
            Previous
          </Link>
        </li>
        <li
          className={`page-item ${styles.grow} disabled`}
          aria-disabled={true}
          aria-hidden={true}
        >
          <a
            className="page-link"
            role="presentation"
            aria-disabled={true}
            aria-hidden={true}
            href="#"
            onClick={(e) => e.preventDefault()}
          >
            &nbsp;
          </a>
        </li>
        <li
          className={`page-item ${nextCursor ? "" : "disabled"}`}
          aria-disabled={nextCursor ? false : true}
        >
          <Link
            className="page-link"
            to={
              nextCursor
                ? makeBookListPath({
                    cursor: {
                      order: BookListNavigationAction.next,
                      index: nextCursor,
                    },
                  })
                : "#"
            }
          >
            Next
          </Link>
        </li>
        <li
          className={`page-item ${nextCursor ? "" : "disabled"}`}
          aria-disabled={nextCursor ? false : true}
        >
          <Link
            className="page-link"
            to={makeBookListPath({
              cursor: {
                order: BookListNavigationAction.previous,
              },
            })}
          >
            Last
          </Link>
        </li>
      </ul>
    </nav>
  );
}
