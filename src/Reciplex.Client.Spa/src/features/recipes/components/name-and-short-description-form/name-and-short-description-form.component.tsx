import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import LinearProgress from "@mui/material/LinearProgress";
import { useController, useForm } from "react-hook-form";
import { useEffect } from "preact/hooks";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";
import Paper from "@mui/material/Paper";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";

/** form for creating a new book/recipe. Also used to modify a book. */
export function NameAndShortDescriptionForm({
  nameLabel,
  shortDescriptionLabel,
  legendText,
  nameMaxLength,
  nameHelpText,
  shortDescriptionHelpText,
  shortDescriptionMaxLength,
  onSuccess,
  pending,
  showError,
  onReset,
  submitText,
  values,
  showConflict,
}: INameAndShortDescriptionFormProps) {
  const { name: initialName, shortDescription: initialShortDescription } =
    values ?? { name: "", shortDescription: "" };
  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
    reset,
  } = useForm<ICreateFormSuccessData>({
    defaultValues: {
      name: initialName,
      shortDescription: initialShortDescription,
    },
  });

  useEffect(() => {
    reset({
      name: initialName,
      shortDescription: initialShortDescription,
    });
  }, [initialName, initialShortDescription, reset]);

  const {
    field: { onChange, onBlur, value, ref },
  } = useController({
    name: "shortDescription",
    control,
    rules: {
      maxLength: {
        value: shortDescriptionMaxLength,
        message: `Max length is ${shortDescriptionMaxLength} characters`,
      },
    },
  });

  const fieldsDisabled = showError || pending || showConflict;
  return (
    <Paper sx={{ p: 2, mt: 2 }}>
      <form onSubmit={handleSubmit(onSuccess)}>
        <Stack spacing={2}>
          <legend>
            <Typography variant="h5" gutterBottom>
              {legendText}
            </Typography>
          </legend>
          {pending && <LinearProgress aria-label="Creating..." />}
          {showConflict && <ConcurrencyConflictAlert onReset={onReset} />}
          {showError && <OperationFailedAlert onRetry={onReset} />}
          <TextField
            label={nameLabel}
            error={!!errors.name}
            helperText={errors?.name?.message ?? nameHelpText}
            fullWidth
            maxLength={nameMaxLength}
            variant="outlined"
            disabled={fieldsDisabled}
            {...register("name", {
              maxLength: {
                value: nameMaxLength,
                message: `Max length is ${nameMaxLength} characters`,
              },
              required: "This field is required",
            })}
          />
          <TextField
            label={shortDescriptionLabel}
            error={!!errors.shortDescription}
            helperText={
              errors?.shortDescription?.message ?? shortDescriptionHelpText
            }
            inputRef={ref}
            multiline
            fullWidth
            variant="outlined"
            value={value}
            onChange={onChange}
            onBlur={onBlur}
            disabled={fieldsDisabled}
          />
          <Stack spacing={1} direction={"row"}>
            <Button
              variant="contained"
              type="submit"
              disabled={showError}
              loading={pending && !showError && !showConflict}
            >
              {submitText}
            </Button>
          </Stack>
        </Stack>
      </form>
    </Paper>
  );
}

/** props for `<NameAndShortDescriptionForm />` */
export interface INameAndShortDescriptionFormProps {
  /** form title. Appears inside `<legend>` */
  legendText: string;
  /** label for the short description text area */
  shortDescriptionLabel: string;
  /** help text for the short description text area */
  shortDescriptionHelpText: string;
  /** label for the name input */
  nameLabel: string;
  /** help text for the name component */
  nameHelpText: string;
  /** max length of name */
  nameMaxLength: number;
  /** max length for short description */
  shortDescriptionMaxLength: number;
  /** invoked on success */
  onSuccess: (data: ICreateFormSuccessData) => void | Promise<void>;
  /** invoked when the reset button is pressed inside of the error banner */
  onReset: () => void;
  /** controls if the pending UI is active */
  pending?: boolean;
  /** shows the action failed banner */
  showError?: boolean;
  /** text for the button */
  submitText: string;
  /** starting values for the form */
  values?: ICreateFormSuccessData;
  /** set to true to show the conflict alert and disable the form */
  showConflict?: boolean;
}

/** data from `onSuccess` */
export interface ICreateFormSuccessData {
  /** user supplied value for name */
  name: string;
  /** user supplied value for short description */
  shortDescription: string;
}
