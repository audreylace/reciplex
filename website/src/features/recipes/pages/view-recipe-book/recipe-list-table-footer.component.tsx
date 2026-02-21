import {
  PaginationControls,
  type PaginationControlsProps,
} from "./pagination-controls.component";

/** footer for the table */
export function RecipeListTableFooter({
  nextCursor,
  previousCursor,
  hasLoaded,
}: {} & PaginationControlsProps) {
  return (
    <tfoot>
      <tr>
        <td colSpan={2}>
          <PaginationControls
            nextCursor={nextCursor}
            previousCursor={previousCursor}
            hasLoaded={hasLoaded}
          />
        </td>
      </tr>
    </tfoot>
  );
}
