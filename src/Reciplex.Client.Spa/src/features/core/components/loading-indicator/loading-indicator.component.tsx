import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";

export function LoadingIndicator({ show }: { show?: boolean }) {
  if (show === false) {
    return null;
  }
  return (
    <Stack
      spacing={2}
      direction="column"
      sx={{ width: "100%", alignItems: "center" }}
    >
      <CircularProgress size="3rem" />
    </Stack>
  );
}
