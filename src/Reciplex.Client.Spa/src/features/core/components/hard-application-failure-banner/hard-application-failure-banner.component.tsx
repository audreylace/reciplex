import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import AlertTitle from "@mui/material/AlertTitle";

/**
 * Banner shown on a critical application error. Has no external dependencies
 * outside of mui ui components.
 */
export function HardApplicationFailureBanner() {
  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button
          color="inherit"
          size="small"
          onClick={() => window.location.assign("/")}
        >
          Back to Reciplex
        </Button>
      }
    >
      <AlertTitle>Something went wrong</AlertTitle>
    </Alert>
  );
}
