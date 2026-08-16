import TableCell from "@mui/material/TableCell";
import IconButton from "@mui/material/IconButton";
import MoreVert from "@mui/icons-material/MoreVert";
import { useId, useState } from "react";
import { BookMenu } from "../book-menu/book-menu.component";

/** table cell with a button for opening a book specific menu */
export function BookMenuButtonCell({
  bookId,
  mayEdit,
  bookName,
  mayShare,
  mayLeave,
  shareKey,
}: IBookMenuButtonCellProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLTableCellElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };
  const menuId = useId();
  const buttonId = useId();

  return (
    <>
      <TableCell role="button" onClick={handleClick}>
        <IconButton
          aria-label={`more for ${bookName}`}
          id={buttonId}
          aria-controls={open ? menuId : undefined}
          aria-expanded={open ? "true" : undefined}
          aria-haspopup="true"
        >
          <MoreVert />
        </IconButton>
      </TableCell>
      <BookMenu
        bookId={bookId}
        mayEdit={mayEdit}
        getAnchorElement={() => anchorEl}
        buttonId={buttonId}
        menuId={menuId}
        onClose={handleClose}
        open={open}
        mayShare={mayShare}
        mayLeave={mayLeave}
        bookName={bookName}
        shareKey={shareKey}
      />
    </>
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
  /** key for sharing */
  shareKey?: string | null;
  /**
   * if the use can invoke the leave action on a book.
   * User must not own the book for this to be available.
   */
  mayLeave: boolean;
}
