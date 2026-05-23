import TableCell from "@mui/material/TableCell";
import IconButton from "@mui/material/IconButton";
import MoreVert from "@mui/icons-material/MoreVert";
import { useId, useRef, useState } from "preact/hooks";
import { BookMenu } from "../book-menu/book-menu.component";

/** table cell with a button for opening a book specific menu */
export function BookMenuButtonCell({
  bookId,
  mayEdit,
  bookName,
  mayShare,
  mayLeave,
}: IBookMenuButtonCellProps) {
  const buttonRef = useRef<HTMLButtonElement>(null);
  const [open, setOpen] = useState(false);
  const handleClick = () => {
    setOpen(true);
  };
  const handleClose = () => {
    setOpen(false);
  };
  const menuId = useId();
  const buttonId = useId();

  return (
    <TableCell onClick={handleClick} role="button">
      <IconButton
        ref={buttonRef}
        aria-label={`more for ${bookName}`}
        id={buttonId}
        aria-controls={open ? menuId : undefined}
        aria-expanded={open ? "true" : undefined}
        aria-haspopup="true"
      >
        <MoreVert />
      </IconButton>
      <BookMenu
        bookId={bookId}
        mayEdit={mayEdit}
        getAnchorElement={() => buttonRef.current}
        buttonId={buttonId}
        menuId={menuId}
        onClose={handleClose}
        open={open}
        mayShare={mayShare}
        mayLeave={mayLeave}
      />
    </TableCell>
  );
}

/** props for `<BookMenuButtonCell />` */
export interface IBookMenuButtonCellProps {
  /** name of the book */
  bookName: string;
  /** if of the book */
  bookId: string;
  /** if the user can edit the book */
  mayEdit: boolean;
  /** if the user can share the book */
  mayShare: boolean;
  /**
   * if the use can invoke the leave action on a book.
   * User must not own the book for this to be available.
   */
  mayLeave: boolean;
}
