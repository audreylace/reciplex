import TableCell from "@mui/material/TableCell";
import IconButton from "@mui/material/IconButton";
import MoreVert from "@mui/icons-material/MoreVert";
import { useId, useRef, useState } from "preact/hooks";
import { RecipeMenu } from "../recipe-menu/recipe-menu.component";

export function RecipeMenuButtonCell({
  recipeId,
  bookId,
  mayEdit,
  recipeName,
}: {
  recipeName: string;
  recipeId: string;
  bookId: string;
  mayEdit: boolean;
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
        aria-label={`more for ${recipeName}`}
        id={buttonId}
        aria-controls={open ? menuId : undefined}
        aria-expanded={open ? "true" : undefined}
        aria-haspopup="true"
      >
        <MoreVert />
      </IconButton>
      <RecipeMenu
        bookId={bookId}
        recipeId={recipeId}
        mayEdit={mayEdit}
        open={open}
        getAnchorElement={() => buttonRef.current}
        menuId={menuId}
        onClose={handleClose}
        buttonId={buttonId}
      />
    </TableCell>
  );
}
