import Typography from "@mui/material/Typography";
import { useForm } from "react-hook-form";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import { ConcurrencyConflictAlert } from "./concurrency-conflict-alert.component";
import { DeleteFailedAlert } from "./delete-failed-alert.component";
import { DeleteAccountFormSkeleton } from "./delete-account-form-skeleton.component";

/** component providing the form for deleting an account */
export function DeleteAccountForm({
  displayName,
  userKey,
  showDeleteError,
  showSkeleton,
  showConcurrencyError,
  pending,
  onDelete,
  onReset,
}: IDeleteAccountFormProps) {
  const {
    register,
    formState: { errors },
    handleSubmit,
  } = useForm<IDeleteAccountFormFields>();

  const onSubmit = handleSubmit(async () => {
    onDelete();
  });

  if (showSkeleton) {
    return <DeleteAccountFormSkeleton />;
  }

  const formDisabled = showDeleteError || pending || showConcurrencyError;
  return (
    <>
      <Typography variant="h4">
        <Stack direction={"row"} gap={2}>
          <span>Deleting account {displayName}</span>
        </Stack>
      </Typography>
      <Typography variant="subtitle1" gutterBottom>
        <Stack direction={"row"} gap={1}>
          <span>Account Key: </span>
          <span>{userKey}</span>
        </Stack>
      </Typography>
      {showConcurrencyError && <ConcurrencyConflictAlert onReset={onReset} />}
      {showDeleteError && <DeleteFailedAlert onReset={onReset} />}
      <form onSubmit={onSubmit}>
        <Stack spacing={2} marginTop={3}>
          <Typography variant="body1" gutterBottom>
            Type {`'${displayName}'`} to delete account
          </Typography>
          <TextField
            label="Display Name"
            helperText={errors.displayName?.message}
            error={!!errors.displayName}
            fullWidth
            variant="filled"
            disabled={formDisabled}
            {...register("displayName", {
              required: true,
              validate: (value) => {
                return displayName === value || `type "${displayName}"`;
              },
            })}
          />
          <Box>
            <Button
              variant="contained"
              type="submit"
              color="error"
              disabled={formDisabled}
              loading={pending}
            >
              Delete Account
            </Button>
          </Box>
        </Stack>
      </form>
    </>
  );
}

/** properties for the main form component */
interface IDeleteAccountFormProps {
  /** the accounts display name */
  displayName: string;
  /** the accounts user key */
  userKey: string;
  /** when true the form appears as a loading skeleton */
  showSkeleton: boolean;
  /** invoked at deletion */
  onDelete: () => void;
  /** invoked by the form as part of error recovery. Indicates likely that the user data should be reloaded. */
  onReset: () => void;
  /** controls if the pending deletion UI is active */
  pending: boolean;
  /** shows the delete failed banner */
  showDeleteError: boolean;
  /** shows the concurrency conflict banner */
  showConcurrencyError: boolean;
}

/** model use by the form control inside of `DeleteAccountForm` */
interface IDeleteAccountFormFields {
  /** account display name */
  displayName: string;
}
