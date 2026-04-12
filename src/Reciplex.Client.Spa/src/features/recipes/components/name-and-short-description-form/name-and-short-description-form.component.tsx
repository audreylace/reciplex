import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import LinearProgress from "@mui/material/LinearProgress";
import Skeleton from "@mui/material/Skeleton";
import { useController, useForm } from "react-hook-form";
import { useEffect } from "preact/hooks";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";

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
  showSkeleton,
  values,
  showConflict,
}: ICreateFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
    setValue,
    getValues,
  } = useForm<ICreateFormSuccessData>({
    defaultValues: values,
  });

  useEffect(() => {
    if (values?.name && getValues("name") !== values?.name) {
      setValue("name", values?.name ?? "", {
        shouldDirty: false,
        shouldTouch: false,
        shouldValidate: false,
      });
    }
    if (
      values?.shortDescription &&
      getValues("shortDescription") !== values?.shortDescription
    ) {
      setValue("shortDescription", values?.shortDescription ?? "", {
        shouldDirty: false,
        shouldTouch: false,
        shouldValidate: false,
      });
    }
  }, [getValues, setValue, values?.name, values?.shortDescription]);

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

  if (showSkeleton) {
    return (
      <NameAndShortDescriptionFormSkeleton
        legendText={legendText}
        submitText={submitText}
      />
    );
  }

  const fieldsDisabled = showError || pending || showConflict;
  return (
    <form onSubmit={handleSubmit(onSuccess)}>
      <Stack spacing={2} marginTop={3}>
        <legend>
          <Typography variant="h5" gutterBottom>
            {legendText}
          </Typography>
        </legend>
        {pending && <LinearProgress aria-label="Creating..." />}
        {showConflict && <ConcurrencyConflictAlert onReset={onReset} />}
        {showError && (
          <Alert
            severity="error"
            variant="filled"
            action={
              <Button color="inherit" size="small" onClick={onReset}>
                Retry
              </Button>
            }
          >
            Something went wrong
          </Alert>
        )}
        <TextField
          label={nameLabel}
          error={!!errors.name}
          helperText={errors?.name?.message ?? nameHelpText}
          fullWidth
          maxLength={nameMaxLength}
          variant="filled"
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
          variant="filled"
          value={value}
          onChange={onChange}
          onBlur={onBlur}
          disabled={fieldsDisabled}
        />
        <Stack gap={1} direction={"row"}>
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
  );
}

/** props for `<CreateForm />` */
export interface ICreateFormProps {
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
  /** when true the form appears as a skeleton */
  showSkeleton?: boolean;
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

/** loading skeleton for the form */
function NameAndShortDescriptionFormSkeleton({
  legendText,
  submitText,
}: INameAndShortDescriptionFormSkeletonProps) {
  return (
    <form onSubmit={(e) => e.preventDefault()}>
      <Stack spacing={2} marginTop={3}>
        <legend>
          <Skeleton>
            <Typography variant="h5" gutterBottom>
              {legendText}
            </Typography>
          </Skeleton>
        </legend>
        <Skeleton width={"100%"}>
          <TextField fullWidth variant="filled" />
        </Skeleton>
        <Skeleton width={"100%"}>
          <TextField multiline fullWidth variant="filled" rows={4} />
        </Skeleton>
        <Stack gap={1} direction={"row"}>
          <Skeleton>
            <Button variant="contained">{submitText}</Button>
          </Skeleton>
        </Stack>
      </Stack>
    </form>
  );
}

/** props for `<NameAndShortDescriptionFormSkeleton />` */
interface INameAndShortDescriptionFormSkeletonProps {
  /** @see ICreateFormProps.legendText */
  legendText: string;
  /** @see ICreateFormProps.submitText */
  submitText: string;
}
