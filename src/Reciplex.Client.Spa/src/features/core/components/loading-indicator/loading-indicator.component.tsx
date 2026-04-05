import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";

export function LoadingIndicator({ show }: { show?: boolean }) {
  if (show === false) {
    return null;
  }
  return (
    <Stack spacing={2} direction="column" alignItems="center" width={"100%"}>
      <CircularProgress size="3rem" />
    </Stack>
  );
}
