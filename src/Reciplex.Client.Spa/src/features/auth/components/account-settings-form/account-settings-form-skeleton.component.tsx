import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";

/**
 * A utility to create a "visual placeholder" that mimics
 * the character count of a typical User Key (approx 8-10 chars)
 */
const KEY_PLACEHOLDER = "wwwwwwwwww";

/**
 * A utility to create a "visual placeholder" that mimics
 * the character count of a typical title
 */
const TITLE_PLACEHOLDER = "wwwwwwwwwwwwwwwwwwwwwwwwwwwwww";

/** skeleton for the form shown while it loads */
export function AccountSettingsFormSkeleton() {
  return (
    <>
      <Typography variant="h4">
        <Stack direction={"row"} gap={2}>
          <span>Modifying account: </span>
          <Skeleton>
            <span>{TITLE_PLACEHOLDER}</span>
          </Skeleton>
        </Stack>
      </Typography>
      <Typography variant="subtitle1" gutterBottom>
        <Stack direction={"row"} gap={1}>
          <span>Account Key: </span>
          <Skeleton>
            <span>{KEY_PLACEHOLDER}</span>
          </Skeleton>
        </Stack>
      </Typography>
      <Stack spacing={2} marginTop={3}>
        <Skeleton width={"100%"}>
          <TextField fullWidth variant="filled" />
        </Skeleton>
        <Skeleton>
          <Box>
            <Button variant="contained">Save</Button>
          </Box>
        </Skeleton>
      </Stack>
    </>
  );
}
