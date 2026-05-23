import { useNavigate } from "react-router";
import {
  makeBookSettingsPath,
  makeCreateRecipePath,
  makeSharePath,
} from "../../route-utils";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Divider from "@mui/material/Divider";
import ListItemIcon from "@mui/material/ListItemIcon";
import AddIcon from "@mui/icons-material/Add";
import SettingsIcon from "@mui/icons-material/Settings";
import { ShareButton } from "./share-button.component";
import ClearIcon from "@mui/icons-material/Clear";

export function BookMenu({
  mayEdit,
  mayShare,
  menuId,
  onClose,
  getAnchorElement,
  open,
  buttonId,
  bookId,
  mayLeave,
}: IRecipeBookMenuProps) {
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
        onClick={() => navigate(makeCreateRecipePath(bookId))}
        disabled={!mayEdit}
      >
        <ListItemIcon>
          <AddIcon fontSize="small" />
        </ListItemIcon>
        Add Recipe
      </MenuItem>
      <Divider />
      <MenuItem
        disabled={!mayEdit}
        onClick={() => {
          navigate(makeBookSettingsPath(bookId));
        }}
      >
        <ListItemIcon>
          <SettingsIcon fontSize="small" />
        </ListItemIcon>
        Settings
      </MenuItem>
      {mayShare && <ShareButton bookId={bookId} />}
      {mayLeave && (
        <MenuItem
          onClick={() => {
            navigate(makeSharePath(bookId, ""));
          }}
        >
          <ListItemIcon>
            <ClearIcon fontSize="small" />
          </ListItemIcon>
          Leave
        </MenuItem>
      )}
    </Menu>
  );
}

/** properties for `RecipeBookMenu` */
export interface IRecipeBookMenuProps {
  /** the id of the book */
  bookId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** if the user can share the book with others */
  mayShare?: boolean;
  /**
   * if the use can invoke the leave action on a book.
   * User must not own the book for this to be available.
   */
  mayLeave?: boolean;
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
