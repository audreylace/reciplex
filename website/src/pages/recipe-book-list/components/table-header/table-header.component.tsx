import styles from "./table-header.module.css";

export function TableHeader() {
  return (
    <thead className={styles.titleWrapper}>
      <tr className={styles.titleRow}>
        <th className={styles.bookName}>Name</th>
        <th className={styles.bookDescription}>Description</th>
        <th
          className={styles.bookIconLink}
          aria-description="column with links to the recipe book"
          role="structure"
          aria-hidden
        >
          &#8203;
        </th>
      </tr>
    </thead>
  );
}
