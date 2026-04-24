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
}: IRecipeBookMenuProps) {
  const navigate = useNavigate();
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
      <Menu
        id={menuId}
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
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
    </div>
  );
}

export interface IRecipeBookMenuProps {
  /** the id of the book */
  bookId: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** if the user has delete privileges */
  mayDelete?: boolean;
}
