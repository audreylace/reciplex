import { useSearchParams } from "react-router";
import type { IPageCursor } from "../../services/recipe-types";
import { PageNavigationButton } from "./page-navigation-button.component";
import { PageSizeSelector } from "./page-size-selector.component";

import styles from "./pagination-controls.module.css";

/** controls for pagination */
export function PaginationControls({
  nextCursor,
  previousCursor,
}: PaginationControlsProps) {
  const setSearchParams = useSearchParams()[1];
  return (
    <div className={styles.paginationWrapper}>
      <PageNavigationButton
        icon="bi bi-skip-start"
        hoverIcon="bi bi-skip-start-fill"
        onClick={() => setSearchParams({})}
        visibilityHidden={!previousCursor}
      />
      <PageNavigationButton
        icon="bi bi-rewind"
        hoverIcon="bi bi-rewind-fill"
        onClick={() => {
          if (previousCursor) {
            setSearchParams({
              at: previousCursor.position,
              source: "previous",
            });
          }
        }}
        visibilityHidden={!previousCursor}
      />
      <div className={styles.paginationGrow}></div>
      <PageSizeSelector />
      <div className={styles.paginationGrow}></div>

      <PageNavigationButton
        icon="bi bi-fast-forward"
        hoverIcon="bi bi-fast-forward-fill"
        onClick={() => {
          if (nextCursor) {
            setSearchParams({ at: nextCursor.position, source: "next" });
          }
        }}
        visibilityHidden={!nextCursor}
      />
      <PageNavigationButton
        icon="bi bi-skip-end"
        hoverIcon="bi bi-skip-end-fill"
        onClick={() => setSearchParams({ source: "previous" })}
        visibilityHidden={!nextCursor}
      />
    </div>
  );
}

/** properties for `PaginationControls` */
export interface PaginationControlsProps {
  /** cursor to move to next page */
  nextCursor?: IPageCursor;
  /** cursor to move to previous page */
  previousCursor?: IPageCursor;
}
