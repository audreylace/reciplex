import type { PropsWithChildren } from "preact/compat";

export function RecipeListTableMessage({ children }: PropsWithChildren<{}>) {
  return (
    <tr>
      <td colSpan={2}>{children}</td>
    </tr>
  );
}
