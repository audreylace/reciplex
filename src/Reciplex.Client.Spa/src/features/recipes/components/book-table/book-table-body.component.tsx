import TableBody from "@mui/material/TableBody";
import type { IRecipeBookModel } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { BookTableRow } from "./book-table-row.component";

export function BookTableBody({
  books,
}: {
  books: IRecipeBookModel[] | undefined;
}) {
  return (
    <TableBody>
      {books && books.length <= 0 && (
        <TableRow>
          <TableCell colSpan={3} align="center">
            No Books
          </TableCell>
        </TableRow>
      )}
      {books?.map((book) => (
        <BookTableRow key={book.id} book={book} />
      ))}
    </TableBody>
  );
}
