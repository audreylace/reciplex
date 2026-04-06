import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

/** error banner shown when the delete fails because of a server error or other exception */
export function DeleteFailedAlert({ onReset }: IConcurrencyConflictAlertProps) {
  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button color="inherit" size="small" onClick={onReset}>
          Retry
        </Button>
      }
    >
      Delete failed
    </Alert>
  );
}

/** properties for `DeleteFailedAlert` */
export interface IConcurrencyConflictAlertProps {
  /** called when the user clicks reload */
  onReset: () => void;
}
