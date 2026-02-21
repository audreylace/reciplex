import { useSearchParams } from "react-router";
import { useRecipeForBookQuery } from "../../hooks/useRecipesForBookQuery.hook";
import { CursorTypes } from "../../services/recipe-types";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";
import { RecipeListTableFooter } from "./recipe-list-table-footer.component";
import { RecipeListHeader } from "./recipe-list-header.component";
import { RecipeListRow } from "./recipe-list-row.componet";
import { RecipeListTableMessage } from "./recipe-list-table-message.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";

import styles from "./recipe-list-table.module.css";

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
        <tbody>
          {recipeListQuery.status === "success" &&
            recipeListQuery.data?.recipes.map((r) => (
              <RecipeListRow key={r.id} recipe={r} />
            ))}
          {recipeListQuery.status === "success" && !recipeListQuery.data && (
            <RecipeListTableMessage>
              <RecipeNotFoundBanner />
            </RecipeListTableMessage>
          )}
          {recipeListQuery.status === "error" && (
            <RecipeListTableMessage>
              <FetchingRecipeFailedBanner />
            </RecipeListTableMessage>
          )}
          {recipeListQuery.status === "pending" && recipeListQuery.isPaused && (
            <RecipeListTableMessage>
              <OfflineBanner />
            </RecipeListTableMessage>
          )}
          {recipeListQuery.status === "pending" &&
            recipeListQuery.fetchStatus === "fetching" && (
              <FetchingRecipeBanner />
            )}
        </tbody>
        <RecipeListTableFooter nextCursor={next} previousCursor={back} />
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
