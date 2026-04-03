import { useCallback, useId, useRef, useState } from "preact/hooks";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { useGetAccountsQuery } from "../../hooks/useGetAccountsQuery.hook";
import { useNavigate } from "react-router";
import {
  useActiveUser,
  useActiveUserKey,
} from "../../hooks/useActiveUser.hook";
import type { IHttpUserJson } from "../../http-clients/users-http-client";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import CardContent from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import Menu from "@mui/material/Menu";
import SettingsIcon from "@mui/icons-material/Settings";
import { Button } from "@mui/material";

export function AccountSelector() {
  const accountQuery = useGetAccountsQuery();

  switch (accountQuery.status) {
    case "error":
    default:
      return <ApplicationErrorBanner />;

    case "pending":
      return <InformationBanner title="Fetching your accounts" />;

    case "success":
      return (
        <>
          <Typography variant="h4" marginBottom={2}>
            Whose Cooking?
          </Typography>
          <Stack spacing={2}>
            <AddAccountButton />
            {accountQuery.data.map((acc) => (
              <AccountCard key={acc.userKey} account={acc} />
            ))}
            {accountQuery.data.length > 0 && <AddAccountButton />}
          </Stack>
        </>
      );
  }
}

function AccountCard({ account }: { account: IHttpUserJson }) {
  const activeUserKey = useActiveUserKey();
  const [open, setOpen] = useState<boolean>(false);
  const buttonRef = useRef<SVGSVGElement | null>(null);
  const navigate = useNavigate();
  const buttonId = useId();

  const setActiveUser = useActiveUser((s) => s.setActiveUser);

  const selectAccount = () => {
    setActiveUser({
      userKey: account.userKey,
      displayName: account.displayName,
    });
  };

  const handleClick = (e: Event) => {
    e.preventDefault();
    setOpen(true);
  };
  const handleClose = (e: Event) => {
    e.preventDefault();
    setOpen(false);
  };

  const active = activeUserKey === account.userKey;
  return (
    <Card variant="outlined" sx={{ display: "flex", flexDirection: "row" }}>
      <Box sx={{ flex: "1 1 auto" }}>
        <CardActionArea
          sx={{
            height: "100%",
          }}
          onClick={selectAccount}
        >
          <CardContent>
            <Stack spacing={1}>
              <Typography variant="h5" component="div">
                <Stack direction="row" spacing={1}>
                  <span>{account.displayName}</span>
                  {active && (
                    <Chip label="active" color="secondary" variant="filled" />
                  )}
                </Stack>
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Key: {account.userKey}
              </Typography>
            </Stack>
          </CardContent>
        </CardActionArea>
      </Box>
      <Box sx={{ flex: "0 0 auto" }}>
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
            <MenuItem
              onClick={() => {
                navigate(
                  `/accounts/${encodeURIComponent(account.userKey)}/settings`,
                );
              }}
            >
              Settings
            </MenuItem>
            <MenuItem onClick={handleClose}>Delete</MenuItem>
          </Menu>
        </CardActionArea>
      </Box>
    </Card>
  );
}

function AddAccountButton() {
  const navigate = useNavigate();
  const createAccountAction = useCallback(
    () => navigate("/accounts/-/sign-up"),
    [navigate],
  );
  return (
    <Box>
      <Button onClick={createAccountAction}>Add Account</Button>
    </Box>
  );
}
