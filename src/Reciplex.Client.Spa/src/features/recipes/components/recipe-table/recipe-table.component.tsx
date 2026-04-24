import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import type { IRecipeModel } from "../../services/recipe-types";
import { RecipeTableHeader } from "./recipe-table-header.component";
import { TablePageControls } from "../table-page-controls/table-page-controls.component";
import { RecipeTableBody } from "./recipe-table-body.component";
import type { Ref } from "preact";
import { forwardRef } from "preact/compat";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";

export const RecipeTable = forwardRef(RecipeTableInner);
function RecipeTableInner(
  {
    pending,
    next,
    previous,
    recipes,
    nextLoading,
    previousLoading,
  }: {
    pending?: boolean;
    next?: string;
    previous?: string;
    recipes: IRecipeModel[] | undefined;
    nextLoading?: boolean;
    previousLoading?: boolean;
  },
  ref?: Ref<HTMLDivElement>,
) {
  const pageSize = useRecipeClientStateContext((s) => s.recipeListPageSize);
  const setPageSize = useRecipeClientStateContext(
    (s) => s.setRecipeListPageSize,
  );
  return (
    <Paper ref={ref}>
      <TableContainer>
        <Table>
          <RecipeTableHeader pending={pending} />
          <RecipeTableBody recipes={recipes} />
        </Table>
      </TableContainer>
      <TablePageControls
        next={next}
        previous={previous}
        nextLoading={nextLoading}
        previousLoading={previousLoading}
        setPageSize={setPageSize}
        pageSize={pageSize}
      />
      {pending && <LinearProgress aria-label="Loading…" />}
    </Paper>
  );
}
