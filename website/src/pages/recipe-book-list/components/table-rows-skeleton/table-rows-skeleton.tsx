import styles from "./table-rows-skeleton.module.css";

/**
 * Renders a set of table rows as a skeleton
 */
export function TableRowsSkeleton({
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
    <tr role="structure" aria-label="loading placeholder">
      <td>
        <span class={styles.glowingShimmerSkeleton}></span>
      </td>
      <td>
        <span
          class={`${styles.glowingShimmerSkeleton} ${styles.fourFifthSkeleton}`}
        ></span>
        <span
          class={`${styles.glowingShimmerSkeleton} ${styles.threeFourSkeleton}`}
        ></span>
      </td>
      <td>
        <span
          class={`${styles.glowingShimmerSkeleton} ${styles.boxSkeleton}`}
        ></span>
      </td>
    </tr>
  );
}
