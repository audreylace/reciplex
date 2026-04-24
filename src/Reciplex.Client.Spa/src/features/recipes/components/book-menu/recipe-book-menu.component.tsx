import { useNavigate } from "react-router";
import { makeCreateRecipePath } from "../../route-utils";
import { useId, useState } from "preact/hooks";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import IconButton from "@mui/material/IconButton";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import Divider from "@mui/material/Divider";
import ListItemIcon from "@mui/material/ListItemIcon";
import AddIcon from "@mui/icons-material/Add";
import SettingsIcon from "@mui/icons-material/Settings";
import { Delete } from "@mui/icons-material";

export function RecipeBookMenuButton({
  bookId,
  mayDelete,
  mayEdit,
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
      <RecipeBookMenu
        open={open}
        onClose={handleClose}
        getAnchorElement={() => anchorEl}
        mayEdit={mayEdit}
        mayDelete={mayDelete}
        bookId={bookId}
        menuId={menuId}
        buttonId={buttonId}
      />
    </div>
  );
}

export interface IRecipeBookMenuButtonProps {
  /** the id of the book */
  bookId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** if the user has delete privileges */
  mayDelete?: boolean;
}

export function RecipeBookMenu({
  mayEdit,
  mayDelete,
  menuId,
  onClose,
  getAnchorElement,
  open,
  buttonId,
  bookId,
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
        onClick={() => {
          navigate(`/books/${bookId}/edit`);
        }}
        disabled={!mayEdit}
      >
        <ListItemIcon>
          <SettingsIcon fontSize="small" />
        </ListItemIcon>
        Settings
      </MenuItem>
      <MenuItem
        onClick={() => {
          navigate(`/books/${bookId}/delete`);
        }}
        disabled={!mayDelete}
      >
        <ListItemIcon>
          <Delete fontSize="small" />
        </ListItemIcon>
        Delete
      </MenuItem>
    </Menu>
  );
}

/** properties for `RecipeBookMenu` */
export interface IRecipeBookMenuProps {
  /** the id of the book */
  bookId: string;
  /** if the user has delete privileges */
  mayDelete?: boolean;
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
