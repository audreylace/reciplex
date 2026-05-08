import { useNavigate } from "react-router";
import {
  makeDeleteRecipePath,
  makeEditRecipePath,
  makeViewRecipePath,
} from "../../route-utils";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Divider from "@mui/material/Divider";
import ListItemIcon from "@mui/material/ListItemIcon";
import Delete from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import LaunchIcon from "@mui/icons-material/Launch";

export function RecipeMenu({
  recipeId,
  bookId,
  mayEdit,
  menuId,
  open,
  onClose,
  buttonId,
  getAnchorElement,
  hideViewRecipeLink,
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
      {!hideViewRecipeLink && (
        <>
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
        </>
      )}
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
  /** Hides the link to the view recipe page */
  hideViewRecipeLink?: boolean;
}
