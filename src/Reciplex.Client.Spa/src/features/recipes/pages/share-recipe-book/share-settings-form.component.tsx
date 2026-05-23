import Switch from "@mui/material/Switch";
import FormControlLabel from "@mui/material/FormControlLabel";
import TextField from "@mui/material/TextField";
import Stack from "@mui/material/Stack";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import AutorenewIcon from "@mui/icons-material/Autorenew";
import InputAdornment from "@mui/material/InputAdornment";
import ButtonGroup from "@mui/material/ButtonGroup";
import IconButton from "@mui/material/IconButton";
import Box from "@mui/material/Box";
import { makeAbsoluteSharePath } from "../../route-utils";
import { ContentWrapper } from "../../../core/components/content-wrapper/content-wrapper.component";
import { ContentTitle } from "../../../core/components/content-title/content-title.component";

/** form for enabling and disabling share links for a book */
export function ShareSettingsForm({
  shareKey,
  onAction,
  bookId,
  disableForm,
}: IShareSettingsFormProps) {
  const absoluteUrl = makeAbsoluteSharePath(bookId, shareKey ?? "");
  return (
    <ContentWrapper>
      <ContentTitle>Modifying invitation settings</ContentTitle>
      <Box sx={{ pb: 3, pt: 1 }}>
        <FormControlLabel
          control={
            <Switch
              checked={!!shareKey}
              onChange={() => {
                if (shareKey) {
                  onAction("clear");
                } else {
                  onAction("regenerate");
                }
              }}
              disabled={disableForm}
            />
          }
          label={shareKey ? "Invites Enabled" : "Invites Disabled"}
        />
      </Box>
      {shareKey && (
        <>
          <Stack
            gap={2}
            direction={"row"}
            sx={{
              alignItems: "center",
            }}
          >
            <TextField
              value={absoluteUrl}
              disabled
              fullWidth
              label={"Invitation Link"}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <ButtonGroup>
                        <IconButton
                          disabled={disableForm}
                          onClick={() => {
                            onAction("regenerate");
                          }}
                        >
                          <AutorenewIcon />
                        </IconButton>
                      </ButtonGroup>
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <ButtonGroup>
                        <IconButton
                          onClick={async () => {
                            await navigator.clipboard.writeText(absoluteUrl);
                          }}
                        >
                          <ContentCopyIcon />
                        </IconButton>
                      </ButtonGroup>
                    </InputAdornment>
                  ),
                },
              }}
            />
          </Stack>
        </>
      )}
    </ContentWrapper>
  );
}

/** props for `<ShareSettingsForm />` */
export interface IShareSettingsFormProps {
  /** book share key */
  shareKey?: string;
  /** the id of the book */
  bookId: string;
  /** invoked to mutate the share key */
  onAction: (kind: "clear" | "regenerate") => void;
  /** disables the form */
  disableForm?: boolean;
}
