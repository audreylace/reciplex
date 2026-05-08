import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

/** error banner shown when the save fails because of a server error or other exception */
export function SaveFailedAlert({ onReset }: IConcurrencyConflictAlertProps) {
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
      Account save failed
    </Alert>
  );
}

/** properties for `SaveFailedAlert` */
export interface IConcurrencyConflictAlertProps {
  /** called when the user clicks reload */
  onReset: () => void;
}
