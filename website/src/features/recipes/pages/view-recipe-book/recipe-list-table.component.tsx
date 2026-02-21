import { useSearchParams } from "react-router";
import { useRecipeForBookQuery } from "../../hooks/useRecipesForBookQuery.hook";
import { CursorTypes } from "../../services/recipe-types";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";
import { RecipeListTableFooter } from "./recipe-list-table-footer.component";
import { RecipeListHeader } from "./recipe-list-header.component";

import styles from "./recipe-list-table.module.css";
import { TableBody } from "./table-body.component";

/**
 * Renders a list of recipes as a table
 * @todo fix skeleton flicker when switching pages and the page size
 */
export function RecipeListTable({ bookId }: { bookId: string }) {
  const size = useRecipeListTableContext((state) => state.size);
  const [searchParams] = useSearchParams();

  const at = searchParams.get("at") ?? undefined;
  const sourceQuery = searchParams.get("source");
  const source =
    sourceQuery === "next"
      ? CursorTypes.next
      : sourceQuery === "previous"
        ? CursorTypes.previous
        : undefined;

  const recipeListQuery = useRecipeForBookQuery(
    bookId,
    source ? { position: at, type: source } : undefined,
    size,
  );

  const next = recipeListQuery.data?.nextCursor;
  const back = recipeListQuery.data?.previousCursor;

  return (
    <div className={styles.recipeWrapper}>
      <table className={styles.recipeTable}>
        <RecipeListHeader />

        <TableBody
          key={`${bookId} / ${at}`}
          fetchStatus={recipeListQuery.fetchStatus}
          recipeData={recipeListQuery.data}
          loadingStatus={recipeListQuery.status}
        />

        <RecipeListTableFooter
          nextCursor={next}
          previousCursor={back}
          hasLoaded={recipeListQuery.status === "success"}
        />
      </table>
    </div>
  );
}

/**
 * Props for `RecipeListTable`
 * @see RecipeListTable
 */
export interface RecipeListTableProps {
  /** the id of the book to load */
  bookId: string;
}
