export function RecipeListTableMessage({
  children,
}: {
  children?: preact.ComponentChildren | undefined;
}) {
  return (
    <tr>
      <td colSpan={2}>{children}</td>
    </tr>
  );
}
