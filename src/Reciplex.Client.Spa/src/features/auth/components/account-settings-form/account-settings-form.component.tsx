import { useForm, type UseFormReturn } from "react-hook-form";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import { SaveFailedAlert } from "./save-failed-alert.component";
import { useEffect } from "preact/hooks";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";
import Paper from "@mui/material/Paper";
import { StackedLabelValue } from "../../../core/components/stacked-label-value/stacked-label-value.component";

/** form for modifying an account */
export function AccountSettingsForm({
  displayName,
  userKey,
  showSaveError,
  showConcurrencyError,
  pending,
  onSave,
  onReset,
}: IAccountSettingsFormProps) {
  const {
    register,
    formState: { errors },
    reset,
    handleSubmit,
  } = useForm<IAccountSettingsFormModel>({
    defaultValues: { displayName: displayName },
  });

  const onSubmit = handleSubmit(async (data) => {
    onSave({
      displayName: data.displayName,
    });
  });

  useEffect(() => {
    reset({ displayName });
  }, [displayName, reset]);

  const formDisabled = showSaveError || pending || showConcurrencyError;
  return (
    <>
      {showConcurrencyError && <ConcurrencyConflictAlert onReset={onReset} />}
      {showSaveError && <SaveFailedAlert onReset={onReset} />}
      <Paper sx={{ p: 2, mt: 2 }}>
        <Stack direction={"column"} gap={2}>
          <Typography variant="h5" gutterBottom>
            Current account settings
          </Typography>
          <StackedLabelValue label="Current Display Name" value={displayName} />
          <StackedLabelValue label="Account Key" value={userKey} gutterBottom />
        </Stack>
      </Paper>
      <Paper sx={{ p: 2, mt: 2 }}>
        <form onSubmit={onSubmit}>
          <legend>
            <Typography variant="h5" gutterBottom>
              Update account settings
            </Typography>
          </legend>
          <Stack spacing={2} marginTop={3}>
            <TextField
              label="New Display Name"
              helperText={displayNameHelpText(errors)}
              error={!!errors.displayName}
              fullWidth
              variant="outlined"
              maxLength={64}
              disabled={formDisabled}
              {...register("displayName", {
                maxLength: 64,
                required: true,
              })}
            />
            <Box>
              <Button
                variant="contained"
                type="submit"
                disabled={formDisabled}
                loading={pending}
              >
                Save
              </Button>
            </Box>
          </Stack>
        </form>
      </Paper>
    </>
  );
}

/** properties for `AccountSettingsForm` */
export interface IAccountSettingsFormProps {
  /** the accounts display name */
  displayName: string;
  /** the accounts user key */
  userKey: string;
  /** invoked at deletion */
  onSave: (args: IOnSaveData) => void;
  /** invoked by the form as part of error recovery. Indicates likely that the user data should be reloaded. */
  onReset: () => void;
  /** controls if the pending deletion UI is active */
  pending: boolean;
  /** shows the save failed banner */
  showSaveError: boolean;
  /** shows the concurrency conflict banner */
  showConcurrencyError: boolean;
}

/** model for useForm hook in `AccountSettingsForm` */
interface IAccountSettingsFormModel {
  displayName: string;
}
export interface IOnSaveData {
  displayName: string;
}

/**
 * Computes the help text for the display name
 * @param errors input errors
 * @returns the help text to show for the input field
 */
function displayNameHelpText(
  errors: UseFormReturn<IAccountSettingsFormModel>["formState"]["errors"],
) {
  if (errors.displayName) {
    if (errors.displayName.type === "required") {
      return "Required to provide a display name to use this app";
    } else if (errors.displayName.type === "maxLength") {
      return "Display name must be no longer than 64 characters";
    }
  }

  return "Name visible to others using the app";
}
