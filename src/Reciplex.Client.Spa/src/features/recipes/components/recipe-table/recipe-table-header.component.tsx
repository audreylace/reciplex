import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";

export function RecipeTableHeader() {
  return (
    <TableHead>
      <TableRow>
        <TableCell>Name</TableCell>
        <TableCell align="right">Description</TableCell>
        <TableCell padding="checkbox"></TableCell>
      </TableRow>
    </TableHead>
  );
}
