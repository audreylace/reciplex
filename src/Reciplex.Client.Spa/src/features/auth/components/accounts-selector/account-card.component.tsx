import Card from "@mui/material/Card";
import {
  useActiveUserKey,
  useActiveUser,
} from "../../hooks/useActiveUser.hook";
import type { IHttpUserJson } from "../../http-clients/users-http-client";
import { AccountMenu } from "./account-menu.component";
import { AccountSelectButton } from "./account-select-button.component";
import Box from "@mui/material/Box";

/** Account card holding an account menu and used to select which account is active */
export function AccountCard({ account }: IAccountCardProps) {
  const activeUserKey = useActiveUserKey();
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const selectAccount = () => {
    setActiveUser({
      userKey: account.userKey,
      displayName: account.displayName,
    });
  };

  return (
    <Card variant="outlined" sx={{ display: "flex", flexDirection: "row" }}>
      <Box sx={{ flex: "1 1 auto" }}>
        <AccountSelectButton
          active={activeUserKey === account.userKey}
          userKey={account.userKey}
          displayName={account.displayName}
          onClick={selectAccount}
        />
      </Box>
      <Box sx={{ flex: "0 0 auto" }}>
        <AccountMenu userKey={account.userKey} />
      </Box>
    </Card>
  );
}

/** properties for  `AccountCard` */
export interface IAccountCardProps {
  /** account this card represents */
  account: IHttpUserJson;
}
