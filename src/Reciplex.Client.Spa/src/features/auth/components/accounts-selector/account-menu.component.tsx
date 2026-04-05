import { useId, useState, useRef } from "preact/hooks";
import { useAccountSettingsNavigate } from "../../hooks/useAccountSettingsNavigate.hook";
import CardContent from "@mui/material/CardContent";
import Menu from "@mui/material/Menu";
import CardActionArea from "@mui/material/CardActionArea";
import MenuItem from "@mui/material/MenuItem";
import SettingsIcon from "@mui/icons-material/Settings";

/** Menu for performing additional account actions  */
export function AccountMenu({ userKey }: { userKey: string }) {
  const buttonId = useId();
  const [open, setOpen] = useState<boolean>(false);
  const buttonRef = useRef<SVGSVGElement | null>(null);
  const [accountPath, accountNavigate] = useAccountSettingsNavigate(
    userKey,
    () => setOpen(false),
  );
  const handleClick = (e: Event) => {
    e.preventDefault();
    setOpen(true);
  };
  const handleClose = (e: Event) => {
    e.preventDefault();
    setOpen(false);
  };

  return (
    <CardActionArea
      aria-controls={open ? buttonId : undefined}
      aria-haspopup="true"
      aria-expanded={open ? "true" : undefined}
      onClick={handleClick}
      sx={{
        height: "100%",
      }}
    >
      <CardContent>
        <SettingsIcon ref={buttonRef} />
      </CardContent>
      <Menu
        id={buttonId}
        anchorEl={() => buttonRef.current}
        open={open}
        onClose={handleClose}
        slotProps={{
          list: {
            "aria-labelledby": buttonId,
          },
        }}
      >
        <MenuItem href={accountPath} onClick={accountNavigate}>
          Settings
        </MenuItem>
        <MenuItem onClick={handleClose}>Delete</MenuItem>
      </Menu>
    </CardActionArea>
  );
}
