import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import type { IRecipeModel } from "../../services/recipe-types";
import { RecipeTableHeader } from "./recipe-table-header.component";
import { RecipeTableTitle } from "./recipe-table-title.component";
import { TablePageControls } from "./table-page-controls.component";
import { RecipeTableBody } from "./recipe-table-body.component";

export function RecipeTable({
  pending,
  next,
  previous,
  recipes,
}: {
  pending?: boolean;
  next?: string;
  previous?: string;
  recipes: IRecipeModel[] | undefined;
}) {
  return (
    <Paper>
      <RecipeTableTitle />
      <TableContainer>
        <Table>
          <RecipeTableHeader />
          <RecipeTableBody recipes={recipes} />
        </Table>
      </TableContainer>
      <TablePageControls next={next} previous={previous} />
      {pending && <LinearProgress aria-label="Loading…" />}
    </Paper>
  );
}
