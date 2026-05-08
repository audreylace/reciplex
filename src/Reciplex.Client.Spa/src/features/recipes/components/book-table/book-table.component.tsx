import { forwardRef, type Ref } from "preact/compat";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import Paper from "@mui/material/Paper";
import LinearProgress from "@mui/material/LinearProgress";
import TableContainer from "@mui/material/TableContainer";
import Table from "@mui/material/Table";
import { TablePageControls } from "../table-page-controls/table-page-controls.component";
import { BookTableHeader } from "./book-table-header.component";
import { BookTableBody } from "./book-table-body.component";
import type { IRecipeBookModel } from "../../services/recipe-types";

export const BookTable = forwardRef(BookTableInner);
function BookTableInner(
  {
    pending,
    next,
    previous,
    nextLoading,
    previousLoading,
    books,
  }: IBookTableProps,
  ref?: Ref<HTMLDivElement>,
) {
  const pageSize = useRecipeClientStateContext((s) => s.bookListPageSize);
  const setPageSize = useRecipeClientStateContext((s) => s.setBookListPageSize);
  return (
    <Paper ref={ref}>
      <TableContainer>
        <Table>
          <BookTableHeader pending={pending} />
          <BookTableBody books={books} />
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

export interface IBookTableProps {
  pending?: boolean;
  next?: string;
  previous?: string;
  nextLoading?: boolean;
  previousLoading?: boolean;
  books?: IRecipeBookModel[];
}
