import Alert from "@mui/material/Alert";
import type { IResetDelegate } from "./types";
import Button from "@mui/material/Button";

/** error banner shown when someone else modifies an account at the same time as the current user  */
export function ConcurrencyConflictAlert({
  onReset,
}: IConcurrencyConflictAlertProps) {
  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button color="inherit" size="small" onClick={onReset}>
          Reload
        </Button>
      }
    >
      Account has been Modified
    </Alert>
  );
}

/** properties for `ConcurrencyConflictAlert` */
export interface IConcurrencyConflictAlertProps {
  /** called when the user clicks reload */
  onReset: IResetDelegate;
}
