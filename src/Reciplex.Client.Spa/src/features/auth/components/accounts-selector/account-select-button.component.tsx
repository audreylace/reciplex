import CardActionArea from "@mui/material/CardActionArea";
import CardContent from "@mui/material/CardContent";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

/** card buttons clicked by the user to select the active account */
export function AccountSelectButton({
  displayName,
  userKey,
  active,
  onClick,
}: IAccountSelectButton) {
  return (
    <CardActionArea
      sx={{
        height: "100%",
      }}
      onClick={onClick}
    >
      <CardContent>
        <Stack spacing={1}>
          <Typography variant="h5" component="div">
            <Stack direction="row" spacing={1}>
              <span>{displayName}</span>
              {active && (
                <Chip label="active" color="secondary" variant="filled" />
              )}
            </Stack>
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Key: {userKey}
          </Typography>
        </Stack>
      </CardContent>
    </CardActionArea>
  );
}

/** properties for `AccountSelectButton` */
export interface IAccountSelectButton {
  /** display name for the account */
  displayName: string;
  /** unique user key */
  userKey: string;
  /** flag indicating if this account is already selected */
  active: boolean;
  /** callback invoked when the user activates this button */
  onClick: () => void;
}
