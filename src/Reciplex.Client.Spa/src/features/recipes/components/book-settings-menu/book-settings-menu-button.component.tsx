import { useId, useState } from "react";
import IconButton from "@mui/material/IconButton";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import ListItemIcon from "@mui/material/ListItemIcon";
import GroupsIcon from "@mui/icons-material/Groups";
import Divider from "@mui/material/Divider";
import { useNavigate } from "react-router";
import Delete from "@mui/icons-material/Delete";
import ShareIcon from "@mui/icons-material/Share";
import {
  makeBookDeletePath,
  makeBookDetailsSettingsPath,
  makeManageAccessPath,
  makeShareSettingsPath,
  makeViewRecipeBookPath,
} from "../../route-utils";
import EditIcon from "@mui/icons-material/Edit";
import SettingsIcon from "@mui/icons-material/Settings";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";

/** Button that opens a settings menu for a book. Should be on all book setting pages */
export function BookSettingsMenuButton({
  bookId,
  mayDelete,
  mayEdit,
  mayManageShareAccess,
}: IBookSettingsMenuButtonProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };
  const menuId = useId();
  const buttonId = useId();
  const navigate = useNavigate();

  return (
    <>
      <div onClick={handleClick}>
        <IconButton
          aria-label="more"
          id={buttonId}
          aria-controls={open ? menuId : undefined}
          aria-expanded={open ? "true" : undefined}
          aria-haspopup="true"
        >
          <SettingsIcon />
        </IconButton>
      </div>
      <Menu
        id={menuId}
        anchorEl={() => anchorEl}
        open={open ?? false}
        onClose={handleClose}
        slotProps={{
          list: {
            "aria-labelledby": buttonId,
          },
        }}
      >
        <MenuItem
          onClick={() => {
            navigate(makeViewRecipeBookPath(bookId));
          }}
        >
          <ListItemIcon>
            <ArrowBackIcon fontSize="small" />
          </ListItemIcon>
          View Book
        </MenuItem>

        <Divider />
        <MenuItem
          disabled={!mayEdit}
          onClick={() => {
            navigate(makeBookDetailsSettingsPath(bookId));
          }}
        >
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          Edit Details
        </MenuItem>
        <MenuItem
          disabled={!mayDelete}
          onClick={() => {
            navigate(makeBookDeletePath(bookId));
          }}
        >
          <ListItemIcon>
            <Delete fontSize="small" />
          </ListItemIcon>
          Delete
        </MenuItem>
        <Divider />
        <MenuItem
          disabled={!mayManageShareAccess}
          onClick={() => {
            navigate(makeShareSettingsPath(bookId));
          }}
        >
          <ListItemIcon>
            <ShareIcon fontSize="small" />
          </ListItemIcon>
          Invitation Settings
        </MenuItem>
        <MenuItem
          disabled={!mayManageShareAccess}
          onClick={() => {
            navigate(makeManageAccessPath(bookId));
          }}
        >
          <ListItemIcon>
            <GroupsIcon fontSize="small" />
          </ListItemIcon>
          Manage Access
        </MenuItem>
      </Menu>
    </>
  );
}

/** props for `<BookSettingsMenuButton />` */
export interface IBookSettingsMenuButtonProps {
  /** the id of the book */
  bookId: string;
  /** can the user manage access to the book */
  mayManageShareAccess: boolean;
  /** can the user delete the book */
  mayDelete: boolean;
  /** can the user edit the book */
  mayEdit: boolean;
}
