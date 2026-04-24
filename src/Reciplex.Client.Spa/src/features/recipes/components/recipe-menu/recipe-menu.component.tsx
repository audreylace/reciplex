import { useNavigate } from "react-router";
import {
  makeDeleteRecipePath,
  makeEditRecipePath,
  makeViewRecipePath,
} from "../../route-utils";
import { useId, useState } from "preact/hooks";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import IconButton from "@mui/material/IconButton";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import Divider from "@mui/material/Divider";
import ListItemIcon from "@mui/material/ListItemIcon";
import { Delete } from "@mui/icons-material";
import EditIcon from "@mui/icons-material/Edit";
import LaunchIcon from "@mui/icons-material/Launch";

export function RecipeMenuButton({
  recipeId,
  bookId,
  mayEdit,
}: IRecipeMenuButtonProps) {
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
      <RecipeMenu
        bookId={bookId}
        recipeId={recipeId}
        mayEdit={mayEdit}
        open={!!anchorEl}
        getAnchorElement={() => anchorEl}
        menuId={menuId}
        onClose={handleClose}
        buttonId={buttonId}
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
}

export function RecipeMenu({
  recipeId,
  bookId,
  mayEdit,
  menuId,
  open,
  onClose,
  buttonId,
  getAnchorElement,
}: IRecipeMenuProps) {
  const navigate = useNavigate();
  return (
    <Menu
      id={menuId}
      anchorEl={getAnchorElement}
      open={open ?? false}
      onClose={onClose}
      slotProps={{
        list: {
          "aria-labelledby": buttonId,
        },
      }}
    >
      <MenuItem
        onClick={() => {
          onClose?.();
          navigate(makeViewRecipePath(bookId, recipeId));
        }}
      >
        <ListItemIcon>
          <LaunchIcon fontSize="small" />
        </ListItemIcon>
        View
      </MenuItem>
      <Divider />
      <MenuItem
        onClick={() => {
          onClose?.();
          navigate(makeEditRecipePath(bookId, recipeId));
        }}
        disabled={!mayEdit}
      >
        <ListItemIcon>
          <EditIcon fontSize="small" />
        </ListItemIcon>
        Edit
      </MenuItem>
      <MenuItem
        onClick={() => {
          onClose?.();
          navigate(makeDeleteRecipePath(bookId, recipeId));
        }}
        disabled={!mayEdit}
      >
        <ListItemIcon>
          <Delete fontSize="small" />
        </ListItemIcon>
        Delete
      </MenuItem>
    </Menu>
  );
}

/** properties for `RecipeMenu` */
export interface IRecipeMenuProps {
  /** the id of the book */
  bookId: string;
  /** the id of the recipe */
  recipeId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** the id of the menu */
  menuId?: string;
  /** true opens menu */
  open?: boolean;
  /** invoked when the menu is closing */
  onClose?: () => void;
  /** id of the button launching the menu */
  buttonId?: string;
  /** invoked to get the element that the button should be anchored against */
  getAnchorElement: () => HTMLElement | null;
}
