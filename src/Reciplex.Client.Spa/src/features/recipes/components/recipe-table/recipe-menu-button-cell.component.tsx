import TableCell from "@mui/material/TableCell";
import IconButton from "@mui/material/IconButton";
import MoreVert from "@mui/icons-material/MoreVert";
import { useId, useState } from "react";
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
    <TableCell role="button">
      <IconButton
        aria-label={`more for ${recipeName}`}
        id={buttonId}
        aria-controls={open ? menuId : undefined}
        aria-expanded={open ? "true" : undefined}
        aria-haspopup="true"
        onClick={handleClick}
      >
        <MoreVert />
      </IconButton>
      <RecipeMenu
        bookId={bookId}
        recipeId={recipeId}
        mayEdit={mayEdit}
        open={open}
        getAnchorElement={() => anchorEl}
        menuId={menuId}
        onClose={handleClose}
        buttonId={buttonId}
      />
    </TableCell>
  );
}
