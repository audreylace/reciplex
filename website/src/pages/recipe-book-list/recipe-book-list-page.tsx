import styles from "./recipe-book-list-page.module.css";
import { PaginationBar } from "./components/pagination-bar/pagination-bar.component";
import { TableHeader } from "./components/table-header/table-header.component";
import { TableBodyContent } from "./components/table-body-content.component";
import { useRecipeBookListPageControl } from "./hooks/useRecipeBookListPageControl.hook";

/**
 * Entry point for recipe book list page component
 */
export function RecipeBookListPage({}: {}) {
  const { isPaused, isPending, data } = useRecipeBookListPageControl();

  return (
    <>
      <main className={styles.pageMain}>
        <h2 className={styles.topLevelHeader}>Recipes Books</h2>
        <div className={styles.recipeBookList}>
          <table
            className={styles.recipeBookListTable}
            aria-description="list of recipes books"
          >
            <TableHeader />
            <tbody className={styles.recipeBookListTableBody}>
              <TableBodyContent
                isPaused={isPaused}
                isPending={isPending}
                data={data}
              />
            </tbody>
          </table>
        </div>
      </main>
      <PaginationBar
        nextCursor={data?.nextCursor?.position}
        previousCursor={data?.previousCursor?.position}
      />
    </>
  );
}
