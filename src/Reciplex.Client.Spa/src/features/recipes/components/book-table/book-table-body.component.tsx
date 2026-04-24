import TableBody from "@mui/material/TableBody";
import type { IRecipeBookModel } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";
import { BookMenuButtonCell } from "./book-menu-button-cell.component";

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

function BookTableRow({ book }: { book: IRecipeBookModel }) {
  const navigate = useNavigate();
  const onClick = () => {
    navigate(makeViewRecipeBookPath(book.id));
  };
  return (
    <TableRow
      hover
      sx={{
        ":hover": {
          cursor: "pointer",
        },
      }}
    >
      <TableCell onClick={onClick} role="button">
        {book.name}
      </TableCell>
      <TableCell onClick={onClick} role="button">
        {book.shortDescription}
      </TableCell>

      <BookMenuButtonCell
        bookId={book.id}
        bookName={book.name}
        mayEdit={book.mayEdit ?? false}
        mayDelete={book.mayDelete ?? false}
      />
    </TableRow>
  );
}
