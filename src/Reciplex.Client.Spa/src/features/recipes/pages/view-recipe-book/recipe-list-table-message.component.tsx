import recipeListTableMessageStylesModule from "./recipe-list-table-message.module.css";

export function RecipeListTableMessage({
  children,
}: {
  children?: preact.ComponentChildren | undefined;
}) {
  return (
    <tr className={recipeListTableMessageStylesModule.tableRow}>
      <td className={recipeListTableMessageStylesModule.tableCell} colSpan={2}>
        {children}
      </td>
    </tr>
  );
}
