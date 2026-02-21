import styles from "./recipe-list-table-rows-skeleton.module.css";
import tableStyles from "./recipe-list-table.module.css";

/**
 * Renders a set of table rows as a skeleton
 */
export function RecipeListTableRowsSkeleton({
  count,
}: {
  /**
   * The number of rows to render
   */
  count: number;
}) {
  const rows = [];
  // tie key to tuple such that if the count changes,
  // then all of the rows get collected and destroyed
  for (let i = 0; i < count; i++) {
    rows.push(<TableRow key={`${count}_${i}`} />);
  }

  return <>{rows}</>;
}

/**
 * Renders a single skeleton row
 */
function TableRow() {
  return (
    <tr
      data-disabled={"true"}
      className={`${tableStyles.recipeListRow}`}
      role="structure"
      aria-label="loading placeholder"
    >
      <td className={tableStyles.tableColumnRecipeDescription}>
        <span className={styles.fadeIn}>
          <span className={styles.glowingShimmerSkeleton}></span>
        </span>
      </td>
      <td className={`${tableStyles.tableColumnRecipeDescription}`}>
        <span className={styles.fadeIn}>
          <span
            className={`${styles.glowingShimmerSkeleton} ${styles.fourFifthSkeleton}`}
          ></span>
          <span
            className={`${styles.glowingShimmerSkeleton} ${styles.threeFourSkeleton}`}
          ></span>
        </span>
      </td>
    </tr>
  );
}
