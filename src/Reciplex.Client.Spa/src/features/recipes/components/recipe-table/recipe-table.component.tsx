import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";
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
    mayEdit,
  }: IRecipeTableProps,
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
          <RecipeTableBody recipes={recipes} mayEdit={mayEdit} />
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

/** props for `<RecipeTable />` */
export interface IRecipeTableProps {
  /** if a load is in progress */
  pending?: boolean;
  /** key for the next page */
  next?: string;
  /** key for the previous page */
  previous?: string;
  /** list of recipes to show on this page */
  recipes: IRecipeListEntryJsonResponse[] | undefined;
  /** if the next page of data is loading */
  nextLoading?: boolean;
  /** if the previous page of data is loading */
  previousLoading?: boolean;
  /** if the user can edit recipes in this book */
  mayEdit: boolean;
}
