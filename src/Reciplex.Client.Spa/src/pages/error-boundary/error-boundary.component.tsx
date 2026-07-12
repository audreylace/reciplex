import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import Backdrop from "@mui/material/Backdrop";
import AlertTitle from "@mui/material/AlertTitle";

export function ErrorBoundary() {
  return (
    <Backdrop open>
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
    </Backdrop>
  );
}
