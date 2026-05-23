import { useId, useState } from "react";
import IconButton from "@mui/material/IconButton";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import { BookMenu } from "../book-menu/book-menu.component";

export function BookMenuButton({
  bookId,
  mayEdit,
  mayShare,
  mayLeave,
}: IRecipeBookMenuButtonProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };
  const menuId = useId();
  const buttonId = useId();

  return (
    <div>
      <IconButton
        aria-label="more"
        id={buttonId}
        aria-controls={open ? menuId : undefined}
        aria-expanded={open ? "true" : undefined}
        aria-haspopup="true"
        onClick={handleClick}
      >
        <MoreVertIcon />
      </IconButton>
      <BookMenu
        open={open}
        onClose={handleClose}
        getAnchorElement={() => anchorEl}
        mayEdit={mayEdit}
        bookId={bookId}
        menuId={menuId}
        buttonId={buttonId}
        mayShare={mayShare}
        mayLeave={mayLeave}
      />
    </div>
  );
}

export interface IRecipeBookMenuButtonProps {
  /** the id of the book */
  bookId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** if the user can share the book with others */
  mayShare?: boolean;
  /** if the user can leave the book */
  mayLeave?: boolean;
}
