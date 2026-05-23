import { useId, useState } from "preact/hooks";
import IconButton from "@mui/material/IconButton";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import { RecipeMenu } from "../recipe-menu/recipe-menu.component";

export function RecipeMenuButton({
  recipeId,
  bookId,
  mayEdit,
  hideViewRecipeLink,
}: IRecipeMenuButtonProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
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
        onClick={(e) => setAnchorEl(e.currentTarget)}
      >
        <MoreVertIcon />
      </IconButton>
      <RecipeMenu
        bookId={bookId}
        recipeId={recipeId}
        mayEdit={mayEdit}
        open={!!anchorEl}
        getAnchorElement={() => anchorEl}
        menuId={menuId}
        onClose={handleClose}
        buttonId={buttonId}
        hideViewRecipeLink={hideViewRecipeLink}
      />
    </div>
  );
}

export interface IRecipeMenuButtonProps {
  /** the id of the book */
  bookId: string;
  /** the id of the recipe */
  recipeId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** Hides the link to the view recipe page */
  hideViewRecipeLink?: boolean;
}
