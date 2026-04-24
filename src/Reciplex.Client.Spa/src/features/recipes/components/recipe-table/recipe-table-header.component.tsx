import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import { RecipeTableTitle } from "./recipe-table-title.component";

export function RecipeTableHeader({ pending }: IRecipeTableHeaderProps) {
  return (
    <TableHead>
      <TableRow>
        <TableCell colSpan={3}>
          <RecipeTableTitle />
        </TableCell>
      </TableRow>
      {!pending && (
        <TableRow>
          <TableCell>Name</TableCell>
          <TableCell>Description</TableCell>
          <TableCell padding="checkbox"></TableCell>
        </TableRow>
      )}
    </TableHead>
  );
}

/**
 * props for `RecipeTableHeader`
 * @see RecipeTableHeader
 */
export interface IRecipeTableHeaderProps {
  /** true when data is loading */
  pending?: boolean;
}
