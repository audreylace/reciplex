import { useId, useState } from "react";
import IconButton from "@mui/material/IconButton";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import ListItemIcon from "@mui/material/ListItemIcon";
import { useNavigate } from "react-router";
import LogoutIcon from "@mui/icons-material/Logout";

/**
 * Menu in the top right corner on the select account page. Holds less common actions like logging out.
 */
export function SelectAccountTopMenuButton() {
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
          <MoreVertIcon />
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
        <MenuItem onClick={() => navigate("/accounts/-/sign-out")}>
          <ListItemIcon>
            <LogoutIcon fontSize="small" />
          </ListItemIcon>
          Sign Out
        </MenuItem>
      </Menu>
    </>
  );
}
