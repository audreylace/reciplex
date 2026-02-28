import { Link, useNavigate } from "react-router";
import {
  BookListNavigationAction,
  makeBookListPath,
} from "../../../../route-utils";
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
      aria-label="page navigation for the list of recipe books"
      className={`${styles.navBar}`}
    >
      <ul>
        <NavLink
          title="go to first page"
          screenReaderLabel={
            !previousCursor
              ? "go to fist page disabled because at first page of books"
              : "go to first page of books"
          }
          direction={BookListNavigationAction.next}
          disabled={previousCursor ? false : true}
        >
          <i className="bi bi-chevron-double-left"></i>
        </NavLink>
        <NavLink
          title="go to previous page"
          index={previousCursor}
          direction={BookListNavigationAction.previous}
          disabled={previousCursor ? false : true}
          screenReaderLabel={
            !previousCursor
              ? "go to previous page disabled because on first page of books"
              : "go to previous page of books"
          }
        >
          <i className="bi bi-chevron-left"></i>
        </NavLink>
        <li
          className={`${styles.expander}`}
          aria-disabled={true}
          aria-hidden={true}
        ></li>
        <NavLink
          title="go to next page"
          index={nextCursor}
          direction={BookListNavigationAction.next}
          disabled={nextCursor ? false : true}
          screenReaderLabel={
            !nextCursor
              ? "go to next disabled because on last page of books"
              : "go to next page of books"
          }
        >
          <i className="bi bi-chevron-right"></i>
        </NavLink>
        <NavLink
          title="go to last page"
          direction={BookListNavigationAction.previous}
          disabled={nextCursor ? false : true}
          screenReaderLabel={
            !nextCursor
              ? "go to last disabled because on final page of books"
              : "go to last page of books"
          }
        >
          <i className="bi bi-chevron-double-right"></i>
        </NavLink>
      </ul>
    </nav>
  );
}

/**
 * Single nav link in the nav bar
 */
function NavLink({
  index,
  direction,
  children,
  disabled,
  screenReaderLabel,
  title,
}: React.PropsWithChildren<{
  /** navigation index */
  index?: string;
  /** if the link is disabled */
  disabled: boolean;
  /** navigation direction */
  direction: BookListNavigationAction;
  screenReaderLabel: string;
  title: string;
}>) {
  const navigation = useNavigate();
  const path = makeBookListPath({
    cursor: {
      order: direction,
      index: index,
    },
  });
  return (
    <li
      onClick={() => {
        // make the whole li clickable for navigation actions
        if (!disabled) {
          navigation(path);
        }
      }}
      title={title}
      data-disabled={disabled}
      aria-disabled={disabled}
      aria-description={screenReaderLabel}
    >
      {!disabled && <Link to={path}>{children}</Link>}
      {disabled && <>{children}</>}
    </li>
  );
}
