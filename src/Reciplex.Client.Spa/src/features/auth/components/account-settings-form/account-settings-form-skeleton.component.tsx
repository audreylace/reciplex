import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";

/** skeleton for the form shown while it loads */
export function AccountSettingsFormSkeleton() {
  return (
    <>
      <Typography variant="h4">
        <Stack direction={"row"} gap={2}>
          <span>Modifying account: </span>
          <Skeleton>
            <span>wwwwwwwwwwwww</span>
          </Skeleton>
        </Stack>
      </Typography>
      <Typography variant="subtitle1" gutterBottom>
        <Stack direction={"row"} gap={1}>
          <span>Account Key: </span>
          <Skeleton>
            <span>wwwwwwwwwwwww</span>
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
