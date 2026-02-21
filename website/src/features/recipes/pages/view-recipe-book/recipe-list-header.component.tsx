import tableStyles from "./recipe-list-table.module.css";
import styles from "./recipe-list-header.module.css";

/** header for the table */
export function RecipeListHeader() {
  return (
    <thead className={styles.recipeHeader}>
      <tr className={tableStyles.recipeListHeaderRow}>
        <th colSpan={2}>
          <h4>Recipes</h4>
        </th>
      </tr>
      <tr
        className={`${tableStyles.recipeListHeaderRow} ${tableStyles.hideSmallScreen}`}
      >
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
  );
}
