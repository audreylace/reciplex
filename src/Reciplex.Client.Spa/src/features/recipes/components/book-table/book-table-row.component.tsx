import TableRow from "@mui/material/TableRow";
import { makeViewRecipeBookPath } from "../../route-utils";
import TableCell from "@mui/material/TableCell";
import { BookMenuButtonCell } from "./book-menu-button-cell.component";
import { useNavigate } from "react-router";
import type { IRecipeBookModel } from "../../services/recipe-types";

export function BookTableRow({ book }: { book: IRecipeBookModel }) {
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
