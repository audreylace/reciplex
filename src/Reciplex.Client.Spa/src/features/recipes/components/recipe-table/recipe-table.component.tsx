import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";
import { RecipeTableHeader } from "./recipe-table-header.component";
import {
  TablePageControls,
  type ITablePageDirectionControlsProps,
} from "../table-page-controls/table-page-controls.component";
import { RecipeTableBody } from "./recipe-table-body.component";
import type { Ref } from "react";
import { forwardRef } from "react";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";

export const RecipeTable = forwardRef(RecipeTableInner);
function RecipeTableInner(
  {
    pending,
    recipes,
    mayEdit,
    searchOnChange,
    searchValue,
    showSearch,
    hideHeader,
    goNext,
    goPrevious,
    nextPending,
    previousPending,
    jumpToEndPending,
    jumpToStartPending,
    jumpToEnd,
    jumpToStart,
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
          <RecipeTableHeader
            hideHeader={hideHeader}
            showSearch={showSearch}
            searchOnChange={searchOnChange}
            searchValue={searchValue}
          />
          <RecipeTableBody recipes={recipes} mayEdit={mayEdit} />
        </Table>
      </TableContainer>
      <TablePageControls
        goNext={goNext}
        goPrevious={goPrevious}
        nextPending={nextPending}
        previousPending={previousPending}
        jumpToEndPending={jumpToEndPending}
        jumpToStartPending={jumpToStartPending}
        jumpToEnd={jumpToEnd}
        jumpToStart={jumpToStart}
        setPageSize={setPageSize}
        pageSize={pageSize}
      />
      {pending && <LinearProgress aria-label="Loading…" />}
    </Paper>
  );
}

/** props for `<RecipeTable />` */
export interface IRecipeTableProps extends ITablePageDirectionControlsProps {
  /** if a load is in progress */
  pending?: boolean;
  /** list of recipes to show on this page */
  recipes: IRecipeListEntryJsonResponse[] | undefined;
  /** if the user can edit recipes in this book */
  mayEdit: boolean;
  /** fired when the search input changes */
  searchOnChange?: (s: string) => void;
  /** value for the search form */
  searchValue?: string;
  /** if the search feature is enabled */
  showSearch?: boolean;
  /** if true, the table header is hidden */
  hideHeader?: boolean;
}
