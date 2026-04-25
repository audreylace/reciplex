import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

export function OperationFailedAlert({ onRetry }: IOperationFailedAlertProps) {
  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button color="inherit" size="small" onClick={onRetry}>
          Retry
        </Button>
      }
    >
      Something went wrong
    </Alert>
  );
}

export interface IOperationFailedAlertProps {
  onRetry: () => void;
}
