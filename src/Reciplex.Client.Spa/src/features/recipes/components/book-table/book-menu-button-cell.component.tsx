import TableCell from "@mui/material/TableCell";
import IconButton from "@mui/material/IconButton";
import MoreVert from "@mui/icons-material/MoreVert";
import { useId, useRef, useState } from "preact/hooks";
import { BookMenu } from "../book-menu/book-menu.component";

export function BookMenuButtonCell({
  bookId,
  mayEdit,
  bookName,
  mayDelete,
}: {
  bookName: string;
  bookId: string;
  mayEdit: boolean;
  mayDelete: boolean;
}) {
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
        mayDelete={mayDelete}
        getAnchorElement={() => buttonRef.current}
        buttonId={buttonId}
        menuId={menuId}
        onClose={handleClose}
        open={open}
      />
    </TableCell>
  );
}
