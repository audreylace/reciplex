import TableRow from "@mui/material/TableRow";
import { makeViewRecipeBookPath } from "../../route-utils";
import TableCell from "@mui/material/TableCell";
import { BookMenuButtonCell } from "./book-menu-button-cell.component";
import { useNavigate } from "react-router";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import Stack from "@mui/material/Stack";
import { SharedRecipeBookIndicatorNoLoad } from "../shared-recipe-book-indicator/shared-recipe-book-indicator-no-load.component";

export function BookTableRow({ book }: { book: IRecipeBookModel }) {
  const navigate = useNavigate();
  const onClick = () => {
    navigate(makeViewRecipeBookPath(book.id));
  };
  const userKey = useActiveUserKey();
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
        <Stack direction="row" sx={{ gap: 1, alignItems: "center" }}>
          {book.name}{" "}
          {book.ownerId !== userKey && (
            <SharedRecipeBookIndicatorNoLoad bookId={book.id} />
          )}
        </Stack>
      </TableCell>
      <TableCell onClick={onClick} role="button">
        {book.shortDescription}
      </TableCell>

      <BookMenuButtonCell
        bookId={book.id}
        bookName={book.name}
        mayEdit={book.mayEdit ?? false}
        mayShare={book.mayShare ?? false}
        mayLeave={book.ownerId !== userKey}
      />
    </TableRow>
  );
}
